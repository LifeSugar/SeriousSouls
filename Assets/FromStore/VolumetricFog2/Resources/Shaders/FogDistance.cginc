#ifndef VOLUMETRIC_FOG_2_FOG_DISTANCE
#define VOLUMETRIC_FOG_2_FOG_DISTANCE

float4 _DistanceData;

half ApplyFogDistance(float3 wpos) {
    float2 vd = _WorldSpaceCameraPos.xz - wpos.xz;
    float voidDistance = dot(vd, vd) * _DistanceData.w;
    half alpha = saturate(1.0 + (voidDistance - 1.0) * _DistanceData.y);
    return alpha;
}

#endif