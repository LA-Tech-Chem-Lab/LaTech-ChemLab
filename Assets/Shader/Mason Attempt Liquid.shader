Shader "Custom/LiquidFillWithTilt"
{
    Properties
    {
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.8
        _LiquidColor ("Liquid Color", Color) = (0, 0, 1, 1)
        _TiltAmount ("Tilt Amount", Range(-1, 1)) = 0.0  // Tilt amount passed from script
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

                // Only apply tilt if there's liquid (i.e., _FillAmount > 0)
                if (_FillAmount > 0.0)
                {
                    // Get the object's rotation in world space (to adjust for tilt)
                    float3 worldRotation = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));

                    // Determine how much tilt there is along the X and Z axes
                    // For example, we use worldRotation.x for the forward/backward tilt and worldRotation.z for side-to-side tilt
                    float tiltFactor = worldRotation.x + worldRotation.z;

                    // Apply the tiltFactor to adjust the liquid's surface level
                    float liquidSurfaceY = lerp(-0.5, 0.5, _FillAmount) + tiltFactor * _TiltAmount;

                    // If the vertex is above the liquid surface, adjust its Y position
                    if (v.vertex.y > liquidSurfaceY)
                    {
                        v.vertex.y = liquidSurfaceY;
                    }
                }
                else
                {
                    // If there's no liquid, place the surface at the bottom
                    v.vertex.y = -0.5; // Set to the bottom of the container when empty
                }

                // Convert the modified vertex position to clip space
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
