Shader "Unlit/Water" {
    Properties {
        _MainTex ("Texture", 2D) = "black" {}
		_NormalTex ("Normal", 2D) = "black" {}
		_ViewDir ("View dir", Vector) = (0, 0, -1, 0)
		_Params ("Depth aspect w/ width, w/ height, refractive index, 0", Vector) = (0.1, 0.1, 1.33, 0)

		_CausticsTex ("Caustics", 2D) = "black" {}
		_CausticsGain ("Caustics Gain", Float) = 0.1
		_CausticsAmbience ("Ambient", Range(0, 1)) = 0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "Water.cginc"

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

			sampler2D _NormalTex;
			float4 _NormalTex_TexelSize;

			sampler2D _CausticsTex;
			float _CausticsGain;
			float _CausticsAmbience;

			float4 _ViewDir;
			float4 _Params; // Depth aspect (d/w, d/h), Refractive index, 0, 0

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
			    #if UNITY_UV_STARTS_AT_TOP
				float scaleY = -1.0;
				#else
				float scaleY = 1.0;
				#endif

                float3 n = tex2D(_NormalTex, i.uv).xyz;
				float3 refrDir = refract(_ViewDir, n, _Params.z);
				//float2 uv = i.uv + (_Params.xy / abs(refrDir.z)) * refrDir.xy * float2(1, scaleY);
				float2 uv = i.uv + UVOffsetByRefraction(_ViewDir, n, _Params.xy, _Params.z);

                float4 cmain = tex2D(_MainTex, uv);
				float intensity = tex2D(_CausticsTex, uv) * _CausticsGain;
				return cmain * lerp(intensity, 1, _CausticsAmbience);
            }
            ENDCG
        }
    }
}
