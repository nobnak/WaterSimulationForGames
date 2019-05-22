﻿Shader "Unlit/Wave" {
    Properties {
        _MainTex ("Texture", 2D) = "black" {}
		_Color ("Color", Color) = (1,0,0,1)
		_Height ("Height", Float) = 1
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

			float4 _Color;
			float _Height;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target {
                float u = tex2D(_MainTex, i.uv).x;
				float4 c1 = 1 - float4(_Color.xyz, 0);
				float t = u / _Height;
				return (t >= 0) ? lerp(0, _Color, t) : lerp(0, c1, -t);
            }
            ENDCG
        }
    }
}