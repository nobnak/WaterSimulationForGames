Shader "Unlit/Caustics_Scan"
{
    Properties
    {
		_Gain ("Gain", Float) = 1
		_Index ("Index", Range(0,6)) = 0
        _Tmp0 ("Tmp 0", 2D) = "black" {}
        _Tmp1 ("Tmp 1", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _Tmp0;
			sampler2D _Tmp1;

			float _Gain;
			int _Index;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
				float4 t0 = tex2D(_Tmp0, i.uv);
				float4 t1 = tex2D(_Tmp1, i.uv);

				float intensity[7] = { t0.r, t0.g, t0.b, t0.a, t1.r, t1.g, t1.b };
				return _Gain * intensity[_Index];
            }
            ENDCG
        }
    }
}
