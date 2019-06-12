Shader "Unlit/Caustics" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
		_Gain ("Gain", Float) = 1

		_TmpTex0 ("Temp texture 0", 2D) = "black" {}
		_TmpTex1 ("Temp texture 1", 2D) = "black" {}
		_Blend ("Blend", Range(0, 1)) = 0
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

			sampler2D _TmpTex0;
			sampler2D _TmpTex1;

			float _Gain;
			float _Blend;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float intensity = tex2D(_MainTex, i.uv).x;
                float4 cc = _Gain * intensity;

				float4 ct0 = tex2D(_TmpTex0, i.uv);
				float4 ct1 = tex2D(_TmpTex1, i.uv);

				return lerp(cc, 0.5 * (ct0 + ct1), _Blend);
            }
            ENDCG
        }
    }
}
