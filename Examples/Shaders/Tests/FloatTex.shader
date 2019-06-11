Shader "Unlit/FloatTex" {
    Properties {
		_TargetTex ("Main Tex", 2D) = "white" {}
		_Comp ("Greater equals", Float) = 0
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

			Texture2D<float> _TargetTex;
			Float _Comp;

            v2f vert (appdata v) {
				uint w, h;
				_TargetTex.GetDimensions(w, h);

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * float2(w, h);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                int v = _TargetTex[int2(i.uv)];
                return v >= _Comp;
            }
            ENDCG
        }
    }
}
