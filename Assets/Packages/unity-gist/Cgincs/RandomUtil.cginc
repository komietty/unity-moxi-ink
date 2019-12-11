#ifndef RANDOMUTIL
#define RANDOMUTIL
#include "ConstantUtil.cginc"
inline float rnd(float2 p) {
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
} 
inline float3 rnd3(float2 p) {
    return 2.0 * (float3(rnd(p * 1), rnd(p * 2), rnd(p * 3)) - 0.5);
}
inline float rndSimple(float i) {
    return frac(sin(dot(float2(i, i * i) * 0.01, float2(12.9898, 78.233))) * 43758.5453);
}
inline float2 rndInsideCircle(float2 p) {
    float d = rnd(p.xy);
    float theta = rnd(p.yx) * PI * 2;
    return d * float2(cos(theta), sin(theta));
}
#endif