Shader "Unlit/FinalRenderShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex ("Texture", 2D) = "white" {}
        _Threshold ("Threshold", Range(0.0, 1.0)) = 0.5
        _Directions ("Directions", Float) = 16.0
        _Quality ("Quality", Float) = 3.0
        _Size ("Size", Float) = 8.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            sampler2D _MaskTex;
            float4 _MaskTex_ST;

            float _Threshold;
            float _Directions;
            float _Quality;
            float _Size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.vertex = UnityPixelSnap(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.uv);
                //float size = _Size * ()
                fixed2 radius = (_Size * (1.0 - mask.r) / _MainTex_TexelSize.zw) ;

                float pi2 = 6.28318530718; // Pi*2
                for (float d = 0.0; d < pi2; d += pi2 / _Directions)
                {
                        for (float dd = 1.0 / _Quality; dd <= 1.0; dd += 1.0 / _Quality)
                        {
                            //col += (mask.r < _Threshold ? 1.0f : 0.0f) * tex2D(_MainTex, i.uv + float2(cos(d), sin(d)) * radius * dd);
                            col += tex2D(_MainTex, i.uv + float2(cos(d), sin(d)) * radius * dd);
                        }
                }

                return col / (_Quality * _Directions);
            }
            ENDCG
        }
    }
}
