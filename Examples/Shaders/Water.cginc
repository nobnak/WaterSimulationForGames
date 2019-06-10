#ifndef __WATER_CGINC__
#define __WATER_CGINC__



#if UNITY_UV_STARTS_AT_TOP
static const float2 Water_UVDir = float2(1.0, -1.0);
#else
static const float2 Water_UVDir = float2(1.0, 1.0);
#endif



float2 UVOffsetByRefraction(float3 _ViewDir, float3 n, float2 depthFieldAspect, float refractive) {
	float3 refrDir = refract(_ViewDir, n, refractive);
	return (depthFieldAspect / abs(refrDir.z)) * refrDir.xy * Water_UVDir;
}


#endif