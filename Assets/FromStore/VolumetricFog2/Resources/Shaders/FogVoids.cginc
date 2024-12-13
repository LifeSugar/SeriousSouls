#ifndef VOLUMETRIC_FOG_2_FOG_VOIDS
#define VOLUMETRIC_FOG_2_FOG_VOIDS

#define FOG_MAX_VOID 8

CBUFFER_START(VolumetricFog2FogVoidBuffers)
    float4 _VF2_FogVoidPositionAndSizes[FOG_MAX_VOID];
    int _VF2_FogVoidCount;
CBUFFER_END


half ApplyFogVoids(float3 wpos) {
    half alpha = 0;
    for (int k=0;k<_VF2_FogVoidCount;k++) {
        float2 vd = _VF2_FogVoidPositionAndSizes[k].xz - wpos.xz;
        float voidDistance = dot(vd, vd) * _VF2_FogVoidPositionAndSizes[k].w;
        //alpha += 1.0 - saturate(lerp(1.0, voidDistance, _VF2_FogVoidPositionAndSizes[k].y));
        alpha += 1.0 - saturate(1.0 + (voidDistance - 1) * _VF2_FogVoidPositionAndSizes[k].y);
    }
    return alpha;
}

#endif