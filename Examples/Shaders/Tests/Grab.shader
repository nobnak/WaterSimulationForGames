Shader "Unlit/Grab" {
    Properties {
		_MainTex ("Main texture", 2D) = "white" {}
		_Blend ("Blend", Range(0, 1)) = 0.5
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

		GrabPass { "_BGTex" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "../Water.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _BGTex;
			float _Blend;

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target {
				float4 cmain = tex2D(_MainTex, i.uv);

				float4 gpos = Water_GrabScreenPosFromLocalUV(i.uv);
                float4 col = tex2Dproj(_BGTex, gpos);

                return lerp(col, cmain, _Blend);
            }
            ENDCG
        }
    }
}
