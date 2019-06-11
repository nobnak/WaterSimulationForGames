Shader "Unlit/UVOffset" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
		_View ("View dir", Vector) = (0, 0, -1, 0)
		_Normal ("Normal", Vector) = (0, 0, 1, 0)
		_Params ("Depth aspect /w, /h, refractive, 0", Vector) = (0.1, 0.1, 1.33, 0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

			float4 _View;
			float4 _Normal;
			float4 _Params;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
				float2 uvoff = Water_UVoffsByRefraction(
					normalize(_View), normalize(_Normal), _Params.xy, _Params.z);
				float2 uv = i.uv + uvoff;
                fixed4 col = tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
}
