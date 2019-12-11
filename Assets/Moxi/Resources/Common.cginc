#ifndef MOXI_LBE_COMMON
#define MOXI_LBE_COMMON
#define THREAD [numthreads(32, 32, 1)]

Texture2D<float4>   Surf_R; // x: water amount, y: ink amount, z: water amount supplyed surf to flow  
Texture2D<float4>   Flow_R; // x: water amount, y: ink amount, z: water amount last frame,
Texture2D<float4>   Fixt_R; // x: none        , y: ink amount, z: none
RWTexture2D<float4> Surf_W;
RWTexture2D<float4> Flow_W;
RWTexture2D<float4> Fixt_W;

#include "../../Packages/unity-gist/Cgincs/SimplexNoise.cginc"
#define brush_head_num 2
float4 _BrushHeads[brush_head_num]; // x, y: pos, z: width
float1 _WtrSupply;
float1 _InkSupply;

bool is_supplied(int2 uv){
    bool f = false;
    for(int i = 0; i < brush_head_num; i++){
        float4 b = _BrushHeads[i];
        f = f || length(uv - b.xy) < b.z + snoise(uv.xyxy * 0.01) * b.z * 0.2;
    }
    return f;
};

#define one_9th  0.11111111111  
#define one_36th 0.02777777777  
#define four_9th 0.44444444444  
#define ll int2(-1,  0)
#define tt int2(0,   1)
#define rr int2(1,   0)
#define bb int2(0,  -1)
#define tl int2(-1,  1)
#define tr int2( 1,  1)
#define br int2( 1, -1)
#define bl int2(-1, -1)
#define ss int2(0,   0)

float _Omega;
int _W;
int _H;

Texture2D<float4>   VelPhi_R;
Texture2D<float4>   ForceV_R;
Texture2D<float4>   ForceD_R;
Texture2D<float4>   ForceR_R;
RWTexture2D<float4> VelPhi_W;
RWTexture2D<float4> ForceV_W;
RWTexture2D<float4> ForceD_W;
RWTexture2D<float4> ForceR_W;

float feq1(int2 dir, float2 v) {
	float1 de = dot(dir, v);
	float1 dv = dot(v, v);
	return 3 * de + 4.5 * de * de - 1.5 * dv;
};

float4 feq4(int2 d0, int2 d1, int2 d2, int2 d3, float2 v){
    return float4(feq1(d0, v), feq1(d1, v), feq1(d2, v), feq1(d3, v));
};

bool condition(uint2 uv){
    return uv.x >= 1 && uv.x < (uint)(_W - 1) && uv.y > 1 && uv.y < (uint)(_H - 1);
}

Texture2D<float4> Grain;
Texture2D<float4> Pinning;

float blocking_factor(uint2 uv){
    return smoothstep(0, 1, clamp(Grain[uv].x, 0.2, 0.8));
};

float4 blocking_factor_avg(uint2 uv, int2 dir0, int2 dir1, int2 dir2, int2 dir3){
    return 0.5 * float4(
        blocking_factor(uv) + blocking_factor(uv + dir0),
        blocking_factor(uv) + blocking_factor(uv + dir1),
        blocking_factor(uv) + blocking_factor(uv + dir2),
        blocking_factor(uv) + blocking_factor(uv + dir3)
    );
};

#define pinning_factor 0.4

bool is_pinning(uint2 uv){
    float rho = Pinning[uv].x * pinning_factor; 
    bool self = VelPhi_R[uv].z <= 0.001;
    bool around = 
            VelPhi_R[uv + ll].z < rho &&
            VelPhi_R[uv + tt].z < rho &&
            VelPhi_R[uv + rr].z < rho &&
            VelPhi_R[uv + bb].z < rho &&
            VelPhi_R[uv + tl].z < rho &&
            VelPhi_R[uv + tr].z < rho &&
            VelPhi_R[uv + br].z < rho &&
            VelPhi_R[uv + bl].z < rho;

    return self && around;
};

#endif