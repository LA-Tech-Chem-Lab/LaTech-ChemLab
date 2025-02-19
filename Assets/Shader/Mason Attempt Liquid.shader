Shader "Custom/LiquidFill"
{
    Properties
    {
        _LiquidColor ("Liquid Color", Color) = (0, 0, 1, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 1.0 // Control the liquid fill level
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }

        Pass
        {
            Stencil
            {
                Ref 1         // Read stencil value of 1
                Comp Equal    // Only render where stencil value is 1
                Pass Keep     // Do not overwrite stencil buffer
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
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
            };

            fixed4 _LiquidColor;
            float _FillAmount; // Liquid fill level

            v2f vert(appdata_t v)
            {
                v2f o;
                // Scale the liquid geometry based on fill amount
                v.vertex.y *= _FillAmount; // Scale down the y position based on fill amount
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                if (_FillAmount == 0)
                {
                    discard; // Skip rendering the liquid if fill amount is 0
                }

                return _LiquidColor; // Blue liquid effect
            }

            ENDCG
        }
    }
}
