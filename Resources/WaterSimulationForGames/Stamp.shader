Shader "Hidden/Stamp" {
    Properties {
        _MainTex ("Texture", 2D) = "black" {}
		_Amp ("Amplitude", Float) = 1
    }
    SubShader {
        Cull Off ZWrite Off ZTest Always

		Blend One One

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _Amp;
			float4x4 _UvMat;

            v2f vert (appdata v) {
				float2 uv = v.uv;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target {
				float2 uv = mul(_UvMat, float4(i.uv, 0, 1)).xy;
                float4 col = tex2D(_MainTex, uv);
                return col * _Amp;
            }
            ENDCG
        }
    }
}
