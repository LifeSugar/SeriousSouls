// The MIT License
// https://www.youtube.com/c/InigoQuilez
// https://iquilezles.org/
// Copyright © 2015 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


// Analytically integrating quadratically decaying participating media within a sphere. 
// No raymarching involved.
//
// Related info: https://iquilezles.org/articles/spherefunctions

//UNITY_SHADER_NO_UPGRADE
#ifndef FOG_INCLUDED
#define FOG_INCLUDED

void ComputeAnalyticalFog_float(float3 fragPos, float3 sphereCenter, float sphereRadius, float3 rayOrigin, float3 rayDir, float depthBuffer,out float Out)
{
    // Normalize the problem to the canonical sphere
    float normalizedDepthBuffer = depthBuffer / sphereRadius;
    float3 rc = (rayOrigin - sphereCenter) / sphereRadius;

    // Find intersection with sphere
    float b = dot(rayDir, rc);
    float c = dot(rc, rc) - 1.0;
    float h = b * b - c;

    // Not intersecting
    if(h < 0.0) 
    {
        Out = 0.0;
        return;
    }

    h = sqrt(h);
    float t1 = -b - h;
    float t2 = -b + h;

    // Not visible (behind camera or behind depth buffer)
    if(t2 < 0.0 || t1 > normalizedDepthBuffer) 
    {
        Out = 0.0;
        return;
    }

    // Clip integration segment from camera to depth buffer
    t1 = max(t1, 0.0);
    t2 = min(t2, normalizedDepthBuffer);

    // Analytical integration of an inverse squared density
    float i1 = -(c * t1 + b * t1 * t1 + t1 * t1 * t1 / 3.0);
    float i2 = -(c * t2 + b * t2 * t2 + t2 * t2 * t2 / 3.0);
    Out = (i2 - i1) * (3.0 / 4.0);
}

#endif 