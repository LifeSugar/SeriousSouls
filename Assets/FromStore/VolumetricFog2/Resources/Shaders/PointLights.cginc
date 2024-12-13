#ifndef VOLUMETRIC_FOG_2_POINT_LIGHTS
#define VOLUMETRIC_FOG_2_POINT_LIGHTS

#if VF2_POINT_LIGHTS

#define FOG_MAX_POINT_LIGHTS 16

CBUFFER_START(VolumetricFog2PointLightBuffers)
    float4 _VF2_FogPointLightPosition[FOG_MAX_POINT_LIGHTS];
    half4 _VF2_PointLightColor[FOG_MAX_POINT_LIGHTS];
    float _VF2_PointLightInsideAtten;
    int _VF2_PointLightCount;
CBUFFER_END

float minimum_distance_sqr(float fogLengthSqr, float3 w, float3 p) {
    // Return minimum distance between line segment vw and point p
    float t = saturate(dot(p, w) / fogLengthSqr); 
    float3 projection = t * w;
    float distSqr = dot(p - projection, p - projection);
    return distSqr;
}

void AddPointLights(inout half4 sum, float3 rayDir, float t0, float fogLength) {
    float3 fogCeilingCut = _WorldSpaceCameraPos.xyz + rayDir * t0;
    fogCeilingCut += rayDir * _VF2_PointLightInsideAtten;
    fogLength -= _VF2_PointLightInsideAtten;
    rayDir *= fogLength;
    float fogLengthSqr = fogLength * fogLength;
    for (int k=0;k<_VF2_PointLightCount;k++) {
        half pointLightInfluence = minimum_distance_sqr(fogLengthSqr, rayDir, _VF2_FogPointLightPosition[k].xyz - fogCeilingCut) / _VF2_PointLightColor[k].w;
        half scattering = sum.a / (1.0 + pointLightInfluence);
        sum.rgb += _VF2_PointLightColor[k].rgb * scattering;
    }
}

half3 GetPointLights(float3 wpos) {
    half3 color = half3(0,0,0);
    for (int k=0;k<FOG_MAX_POINT_LIGHTS;k++) {
        float3 toLight = _VF2_FogPointLightPosition[k].xyz - wpos;
        float dist = dot(toLight, toLight);
        color += _VF2_PointLightColor[k].rgb * _VF2_PointLightColor[k].w / dist;
    }
    return color;
}

#endif

#endif