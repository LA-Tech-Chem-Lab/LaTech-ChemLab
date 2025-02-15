Shader "Custom/FullyTransparentBeaker"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 0) // Default alpha is 0 (transparent)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }

        Pass
        {
            // Make it fully transparent by setting alpha to 0
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off     // Disable depth writing, since it's fully transparent
            ZTest LEqual   // Allow the transparent object to be in the scene but not affect depth

            Stencil
            {
                Ref 1           // Reference value for stencil
                Comp Always     // Always write to stencil
                Pass Replace    // Replace stencil value with Ref (1)
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
            };

            fixed4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(_Color.rgb, 0); // Set alpha to 0, making it fully transparent
            }
            ENDCG
        }
    }
}
