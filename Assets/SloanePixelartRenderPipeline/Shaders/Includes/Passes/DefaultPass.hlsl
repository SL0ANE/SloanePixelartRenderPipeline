#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "../Inputs/Buffers.hlsl"
#include "../Inputs/Structures.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    float4 _RimLightColor;
    int _LocalUnitScale;
    float _Smoothness;
    float _Metallic;
    float _NormalBlendScale;
    float _NormalEdgeThreshold;
    int _MainLightLevel;

    sampler2D _BaseMap;
    float4 _BaseMap_ST;

    sampler2D _DiffuseDitherPalette;
    float4 _DiffuseDitherPalette_ST;
    float _DiffuseDitherStrength;

    float _Priority;
    sampler2D _PriorityMap;
    float4 _PriorityMap_ST;
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(float4x4, _SnapOffset)
UNITY_INSTANCING_BUFFER_END(Props)

#include "CommonPass.hlsl"

void DefaultFrag(Varyings input, out float4 outAlbedo : BUFFER_ALBEDO, out float4 outNormal: BUFFER_NORMAL, out float4 outPhysical: BUFFER_PHYSICAL, out float4 outShape: BUFFER_SHAPE, out float4 outPalette: BUFFER_PALETTE, out float4 outRimLight: BUFFER_RIMLIGHT, out float4 outLightmapUV: BUFFER_LIGHTMAP_UV)
{
    outAlbedo = tex2D(_BaseMap, input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw);
    outAlbedo *= _BaseColor;
    
    outNormal = float4(normalize(input.normalWS), 1.0);

    float4 physicalPropOutput = float4(0.0, 0.0, 0.0, 0.0);
    float4 shapePropOutput = float4(0.0, 0.0, 0.0, 0.0);
    float4 palettePropOutput = float4(0.0, 0.0, 0.0, 0.0);

    float2 screenPos = input.positionSS.xy / input.positionSS.w;

    physicalPropOutput.r = _Smoothness;
    physicalPropOutput.g = _Metallic;

    palettePropOutput.r = float(_MainLightLevel) / 255.0;
    palettePropOutput.g = tex2D(_DiffuseDitherPalette, screenPos * _ScreenParams.xy / _DiffuseDitherPalette_ST.xy).r;
    palettePropOutput.g = (palettePropOutput.g * 2.0 - 1.0) * _DiffuseDitherStrength;
    palettePropOutput.g = palettePropOutput.g * 0.5 + 0.5;

    shapePropOutput.r = _Priority * tex2D(_PriorityMap, input.uv * _PriorityMap_ST.xy + _PriorityMap_ST.zw).r;
    shapePropOutput.g = _NormalBlendScale;
    shapePropOutput.b = _NormalEdgeThreshold;

    outPhysical = physicalPropOutput;
    outShape = shapePropOutput;
    outPalette = palettePropOutput;
    outRimLight = _RimLightColor;
    outLightmapUV = float4(input.staticLightmapUV, input.dynamicLightmapUV);
}