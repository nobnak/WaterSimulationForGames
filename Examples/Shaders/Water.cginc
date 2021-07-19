#ifndef __WATER_CGINC__
#define __WATER_CGINC__



#if UNITY_UV_STARTS_AT_TOP
static const float YDir = -1.0;
#else
static const float YDir = 1.0;
#endif

static const float2 Water_UVDir = float2(1.0, YDir);



float4 Water_UVtoLocal(float2 uv) {
	float2 local = (uv - 0.5);
	return float4(local, 0, 1);
}

float4 Water_GrabScreenPosFromLocalUV(float2 uv) {
	float4 lpos = Water_UVtoLocal(uv);
	float4 cpos = UnityObjectToClipPos(lpos);
	float4 gpos = ComputeGrabScreenPos(cpos);
	return gpos;
}

float2 Water_UVoffsByRefraction(float3 view, float3 n, float2 depthFieldAspect, float refractive) {
	float3 refrDir = refract(view, n, refractive);
	return (depthFieldAspect / abs(refrDir.z)) * refrDir.xy;
}
float2 Water_UVoffsByRefraction2(float3 view, float3 n, float4 params) {
	return Water_UVoffsByRefraction(view, n, params.xy, params.z);
}


#endif