Shader "Unlit/Wave" {
    Properties {
        _MainTex ("Texture", 2D) = "black" {}
		_Gain ("Gain", Float) = 1
    }
    SubShader {
        Tags { "RenderType"="Opaque" }

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
            float4 _MainTex_ST;

			float _Gain;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float u = tex2D(_MainTex, i.uv).x;
				float t = u * _Gain;
				return float4(saturate(t), saturate(-t), 1 - abs(t), 1);
            }
            ENDCG
        }
    }
}
