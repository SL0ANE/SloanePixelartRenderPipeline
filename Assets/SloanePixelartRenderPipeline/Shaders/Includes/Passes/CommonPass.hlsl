#include "../Inputs/CameraParams.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#define UNITSNAP(coord, size) round(coord / size) * size

Varyings PixelartBaseVert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);

    float4x4 modelMatrix = GetObjectToWorldMatrix();
    float4x4 snapOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _SnapOffset);
    bool hasSnapOffset = snapOffset._m33 == 1.0;
    if(hasSnapOffset) modelMatrix = mul(snapOffset, modelMatrix);

#ifdef ALIGN_TO_PIXEL
    float3 originWS = mul(modelMatrix, float4(0.0, 0.0, 0.0, 1.0)).xyz;
    float3 originVS = mul(PIXELART_CAMERA_MATRIX_V, float4(originWS, 1.0));
#ifdef UNIT_SCALE
    float unitSize = _UnitSize / _LocalUnitScale;
#else
    float unitSize = _UnitSize;
#endif
    float3 originVSAligned = float3(UNITSNAP(originVS.x, unitSize), UNITSNAP(originVS.y, unitSize), originVS.z);
    float3 originVSOffset = originVSAligned - originVS;
#endif

    output.positionWS = mul(modelMatrix, float4(input.positionOS.xyz, 1.0)).xyz;

#ifdef ALIGN_TO_PIXEL
    output.positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(output.positionWS, 1.0)) + originVSOffset;
#else
    output.positionVS = mul(PIXELART_CAMERA_MATRIX_V, float4(output.positionWS, 1.0));
#endif
    output.positionCS = mul(GetViewToHClipMatrix(), float4(output.positionVS, 1.0));

    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif

    output.positionSS = ComputeScreenPos(output.positionCS);
    output.positionSS.xy /= output.positionSS.w;

#ifdef ALIGN_TO_PIXEL
    float4 originSS = ComputeScreenPos(mul(GetViewToHClipMatrix(), float4(originVSAligned, 1.0)));
    originSS.xy /= originSS.w;
    output.positionSSO = output.positionSS.xy - originSS.xy;
#else
#endif

    output.uv = input.uv;

    return output;
}