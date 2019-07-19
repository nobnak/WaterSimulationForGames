Shader "Unlit/WaterScreen" {
    Properties {
		_WaterColor ("Water color", Color) = (1,1,1,1)
		_WaterMaskTex ("Water mask", 2D) = "white" {}
		_WaterMaskShift ("Water mask shift", Vector) = (1, 1, 0, 0)

		_NormalTex ("Normal", 2D) = "black" {}
		_ViewDir ("View dir", Vector) = (0, 0, -1, 0)
		_Params ("Depth aspect w/ width, w/ height, refractive index, 0", Vector) = (0.1, 0.1, 1.33, 0)

		_CausticsTex ("Caustics", 2D) = "white" {}
		_CausticsGain ("Caustics Gain", Float) = 0.1
		_CausticsAmbience ("Ambient", Range(0, 1)) = 0
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		ZTest LEqual ZWrite Off Cull Off
		
		GrabPass { "_GrabTex" }
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

			float4 _WaterColor;
			sampler2D _WaterMaskTex;
			float4 _WaterMaskShift;

			sampler2D _NormalTex;
			float4 _NormalTex_TexelSize;

			sampler2D _CausticsTex;
			float _CausticsGain;
			float _CausticsAmbience;

			float4 _ViewDir;
			float4 _Params; // Depth aspect (d/w, d/h), Refractive index, 0, 0

            sampler2D _GrabTex;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float3 n = tex2D(_NormalTex, i.uv).xyz;
				float2 uv = i.uv + Water_UVoffsByRefraction2(_ViewDir, n, _Params);

				float4 gpos = Water_GrabScreenPosFromLocalUV(uv);
                float4 cmain = tex2Dproj(_GrabTex, gpos);
				float4 cmask = tex2D(_WaterMaskTex, _WaterMaskShift.xy * i.uv + _WaterMaskShift.zw);

				float intensity = tex2D(_CausticsTex, uv) * _CausticsGain;
				return cmain * cmask * _WaterColor * lerp(intensity, 1, _CausticsAmbience);
            }
            ENDCG
        }
    }
}
