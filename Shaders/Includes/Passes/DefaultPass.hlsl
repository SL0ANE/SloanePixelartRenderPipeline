#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "../Inputs/Buffers.hlsl"
#include "../Inputs/Structures.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    int _LocalUnitScale;
    float _Smoothness;
    float _Metallic;
    float _NormalBlendScale;
    int _MainLightLevel;
CBUFFER_END

#include "CommonPass.hlsl"

void DefaultFrag(Varyings input, out float4 outAlbedo : BUFFER_ALBEDO, out float4 outNormal: BUFFER_NORMAL, out float4 outPhysical: BUFFER_PHYSICAL, out float4 outShape: BUFFER_SHAPE, out float4 outPalette: BUFFER_PALETTE)
{
    outAlbedo = _BaseColor;
    outNormal = float4(normalize(input.normalWS), 1.0);

    float4 physicalPropOutput = float4(0.0, 0.0, 0.0, 0.0);
    float4 shapePropOutput = float4(0.0, 0.0, 0.0, 0.0);
    float4 palettePropOutput = float4(0.0, 0.0, 0.0, 0.0);

    physicalPropOutput.r = _Smoothness;
    physicalPropOutput.g = _Metallic;

    palettePropOutput.r = float(_MainLightLevel) / 255.0;

    shapePropOutput.r = _NormalBlendScale;

    outPhysical = physicalPropOutput;
    outShape = shapePropOutput;
    outPalette = palettePropOutput;
}