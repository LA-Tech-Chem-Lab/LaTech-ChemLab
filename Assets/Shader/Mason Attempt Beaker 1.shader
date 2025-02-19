Shader "Custom/FullyTransparentBeaker"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Geometry" }

        Pass
        {
            Stencil
            {
                Ref 1         // Set stencil value to 1
                Comp Always   // Always pass the stencil test
                Pass Replace  // Replace stencil value with 1
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
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

            fixed4 _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(0, 0, 0, 0); // Completely transparent, writes to depth
            }
            ENDCG
        }
    }
}
