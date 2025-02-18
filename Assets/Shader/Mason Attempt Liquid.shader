Shader "Custom/LiquidFill"
{
    Properties
    {
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.8
        _LiquidColor ("Liquid Color", Color) = (0, 0, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Stencil
            {
                Ref 1        // Must match the beaker's stencil reference
                Comp Equal   // Only render where the stencil value is 1
                Pass Keep
            }

            ZWrite On  // Ensures proper depth sorting
            ZTest LEqual  

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

            // Vertex shader
            v2f vert(appdata_t v)
            {
                v2f o;

                // Calculate liquid surface height
                float liquidSurfaceY = lerp(-0.5, 0.5, _FillAmount); // Adjust this range for your beaker's size.

                // Prevent liquid from rendering outside beaker by clamping height
                v.vertex.y = min(v.vertex.y, liquidSurfaceY);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            // Fragment shader
            fixed4 frag(v2f i) : SV_Target
            {
                // Return the liquid color with full opacity
                return _LiquidColor;
            }

            ENDCG
        }
    }
    Fallback "Diffuse"
}
