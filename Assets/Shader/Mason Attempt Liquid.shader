Shader "Custom/LiquidFillWithTilt"
{
    Properties
    {
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.8
        _LiquidColor ("Liquid Color", Color) = (0, 0, 1, 1)
        _TiltAmount ("Tilt Amount", Range(-1, 1)) = 0.0  // Supports both left & right tilt
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            fixed4 _LiquidColor;
            float _FillAmount;
            float _TiltAmount;

            v2f vert(appdata_t v)
            {
                v2f o;
                
                // Get object scale in world space
                float3 worldScale = float3(
                    length(unity_ObjectToWorld._m00_m10_m20),
                    length(unity_ObjectToWorld._m01_m11_m21),
                    length(unity_ObjectToWorld._m02_m12_m22)
                );

                // Find the vertical position of the liquid surface, affected by tilt
                float tiltOffset = _TiltAmount * (v.vertex.x / worldScale.x);
                float liquidSurfaceY = lerp(-0.5 * worldScale.y, 0.5 * worldScale.y, _FillAmount) + tiltOffset;

                // If vertex is above liquid level, clamp it down
                if (v.vertex.y > liquidSurfaceY)
                {
                    v.vertex.y = liquidSurfaceY;
                }

                // Convert to clip space
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _LiquidColor;
            }
            ENDCG
        }
    }
}
