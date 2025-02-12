Shader "Custom/LiquidStencil"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 1, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+1" }
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref 1
            Comp Equal
        }

        Pass
        {
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

            float _FillAmount;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Clip the liquid at the given height (_FillAmount)
                if (i.worldPos.y < _FillAmount)
                    discard;

                return fixed4(0, 0, 1, 0.8); // Blue color
            }
            ENDCG
        }
    }
}

