#include "../Inputs/CameraParams.hlsl"
#define UNITSNAP(coord, size) round(coord / size) * size

Varyings PixelartBaseVert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);

#ifdef ALIGN_TO_PIXEL
    float3 originWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
    float3 originVS = mul(PIXELART_CAMERA_MATRIX_V, float4(originWS, 1.0));
#ifdef UNIT_SCALE
    float unitSize = _UnitSize / _LocalUnitScale;
    float3 originVSOffset = float3(UNITSNAP(originVS.x, unitSize), UNITSNAP(originVS.y, unitSize), originVS.z) - originVS;
#else
    float3 originVSOffset = float3(UNITSNAP(originVS.x, _UnitSize), UNITSNAP(originVS.y, _UnitSize), originVS.z) - originVS;
#endif
#endif

    output.positionWS = TransformObjectToWorld(input.positionOS.xyz);

#ifdef ALIGN_TO_PIXEL
    output.positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(output.positionWS, 1.0)) + originVSOffset;
    // output.positionVS = float3(UNITSNAP(output.positionVS.x, _UnitSize), UNITSNAP(output.positionVS.y, _UnitSize), output.positionVS.z);
#else
    output.positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(output.positionWS, 1.0));
#endif
    output.positionCS = mul(GetViewToHClipMatrix(), float4(output.positionVS, 1.0));

/* #ifdef ALIGN_TO_PIXEL
    float3 originWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
    float3 cameraRight = float3(PIXELART_CAMERA_MATRIX_V._m00, PIXELART_CAMERA_MATRIX_V._m10, PIXELART_CAMERA_MATRIX_V._m20);
    float3 cameraUp = float3(PIXELART_CAMERA_MATRIX_V._m01, PIXELART_CAMERA_MATRIX_V._m11, PIXELART_CAMERA_MATRIX_V._m21);

    float xCoord = dot(originWS, cameraRight) - PIXELART_CAMERA_MATRIX_V._m03;
    float yCoord = dot(originWS, cameraUp)- PIXELART_CAMERA_MATRIX_V._m13;
#ifdef UNIT_SCALE
    float unitSize = _UnitSize / _LocalUnitScale;
    float3 originWSOffset = (UNITSNAP(xCoord, unitSize) - xCoord) * cameraRight + (UNITSNAP(yCoord, unitSize) - yCoord) * cameraUp;
#else
    float3 originWSOffset = (UNITSNAP(xCoord, _UnitSize) - xCoord) * cameraRight + (UNITSNAP(yCoord, _UnitSize) - yCoord) * cameraUp;
#endif
#endif

    output.positionWS = TransformObjectToWorld(input.positionOS.xyz) + originWSOffset;
    output.positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(output.positionWS, 1.0));
    output.positionCS = mul(GetViewToHClipMatrix(), float4(output.positionVS, 1.0)); */

    // output.normalWS = float3(originVSOffset) / unitSize / 0.5 + 0.5;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);

    output.uv = input.uv;

    return output;
}