Shader "Unlit/Refract" {
    Properties {
        _MainTex ("Texture", 2D) = "black" {}
		_NormalTex ("Normal", 2D) = "black" {}
		_ViewDir ("View dir", Vector) = (0, 0, -1, 0)
		_Refractive ("Refractive index", Float) = 1.33
		_Aspect ("Aspect (H/W)", Float) = 1
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

			sampler2D _NormalTex;
			float4 _NormalTex_TexelSize;

			float4 _ViewDir;
			float _Refractive;
			float _Aspect;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float3 n = tex2D(_NormalTex, i.uv).xyz;
				float3 refrDir = refract(_ViewDir, n, _Refractive);
				float2 uv = i.uv + (_Aspect / abs(refrDir.z)) * refrDir.xy;

                float4 cmain = tex2D(_MainTex, uv);
				return cmain;
            }
            ENDCG
        }
    }
}
