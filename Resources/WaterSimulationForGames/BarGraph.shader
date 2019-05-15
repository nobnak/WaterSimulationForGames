Shader "Unlit/BarGraph" {
    Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Params ("Params", Vector) = (1,1,1,1)
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

			float4 _Color;
			float4 _Params;

			StructuredBuffer<float> _Values;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
				float2 uv = i.uv;
				uint key = (uint)(uv.x * _Params.x + _Params.y);
				float v = _Values[key] * _Params.z + _Params.w;
				return ((_Params.w < v) ? (_Params.w < uv.y && uv.y < v) : (v < uv.y && uv.y < _Params.w)) ? _Color : 0;
            }
            ENDCG
        }
    }
}
