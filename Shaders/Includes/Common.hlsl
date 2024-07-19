#include "Buffers.hlsl"

CBUFFER_START(UnityMetaPass)
#ifdef TEXTURE_BASED
    
#endif
CBUFFER_END

float _UnitSize;

#include "Structures.hlsl"

#define UNITSNAP(coord, size) round(coord / size) * size

Varyings PixelartBaseVert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);

#ifdef ALIGN_TO_PIXEL
    float3 originWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
    float3 originVS = TransformWorldToView(originWS);
#ifdef UNIT_SCALE
    float unitSize = _UnitSize / _LocalUnitScale;
    float3 originVSOffset = float3(UNITSNAP(originVS.x, unitSize), UNITSNAP(originVS.y, unitSize), originVS.z) - originVS;
#else
    float3 originVSOffset = float3(UNITSNAP(originVS.x, _UnitSize), UNITSNAP(originVS.y, _UnitSize), originVS.z) - originVS;
#endif
#endif

    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);

#ifdef ALIGN_TO_PIXEL
    output.positionVS = TransformWorldToView(output.positionWS) + originVSOffset;
    // output.positionVS = float3(UNITSNAP(output.positionVS.x, _UnitSize), UNITSNAP(output.positionVS.y, _UnitSize), output.positionVS.z);
#else
    output.positionVS = TransformWorldToView(output.positionWS);
#endif
    output.positionCS = mul(GetViewToHClipMatrix(), float4(output.positionVS, 1.0));

    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);

    output.uv = input.uv;

    return output;
}