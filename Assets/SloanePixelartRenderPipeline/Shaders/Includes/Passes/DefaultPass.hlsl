#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "../Inputs/Buffers.hlsl"
#include "../Inputs/Structures.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    float4 _RimLightColor;
    int _LocalUnitScale;
    float _Smoothness;
    float _Metallic;
    float _NormalEdgeThreshold;
    float _MainLightLevel;
    int _EdgeLevel;

    sampler2D _BaseMap;
    float4 _BaseMap_ST;

    sampler2D _BumpMap;
    float4 _BumpMap_ST;
    float _BumpScale;

    sampler2D _DiffuseDitherPalette;
    float4 _DiffuseDitherPalette_ST;
    float _DiffuseDitherStrength;

    float _Priority;
    sampler2D _PriorityMap;
    float4 _PriorityMap_ST;

    float _AAScale;

    int _ApplyOutline;
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(float4x4, _SnapOffset)
UNITY_INSTANCING_BUFFER_END(Props)

#include "CommonPass.hlsl"

void DefaultFrag(Varyings input, out float4 outAlbedo : BUFFER_ALBEDO, out float4 outNormal0: BUFFER_NORMAL0, out float4 outNormal1: BUFFER_NORMAL1, out float4 outPhysical: BUFFER_PHYSICAL, out float4 outShape: BUFFER_SHAPE, out float4 outPalette: BUFFER_PALETTE, out float4 outRimLight: BUFFER_RIMLIGHT, out float4 outLightmapUV: BUFFER_LIGHTMAP_UV)
{
    outAlbedo = tex2D(_BaseMap, input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw);
    clip(outAlbedo.a - 0.003922);
    outAlbedo *= _BaseColor;

    float3 screenSpaceNormal = normalize(cross(ddy(input.positionWS) , ddx(input.positionWS)));
    outNormal0 = float4(screenSpaceNormal, 1.0);

    float3 normalTS = UnpackNormalScale(tex2D(_BumpMap, input.uv * _BumpMap_ST.xy + _BumpMap_ST.zw), _BumpScale);
    float sgn = input.tangentWS.w;
    float3 normalWS = normalize(input.normalWS.xyz);
    float3 tangentWS = normalize(input.tangentWS.xyz);
    float3 bitangent = sgn * cross(normalWS.xyz, tangentWS.xyz);
    normalWS = TransformTangentToWorld(normalTS, float3x3(tangentWS.xyz, bitangent, normalWS));
    
    outNormal1 = float4(normalize(normalWS.xyz), 1.0);

    float4 physicalPropOutput = float4(0.0, 0.0, 0.0, 0.0);
    float4 shapePropOutput = float4(0.0, 0.0, 0.0, 0.0);
    float4 palettePropOutput = float4(0.0, 0.0, 0.0, 0.0);

    float2 screenPos = input.positionSSO;

    physicalPropOutput.r = _Smoothness;
    physicalPropOutput.g = _Metallic;

    palettePropOutput.r = float(_MainLightLevel) / 255.0;
    palettePropOutput.g = tex2D(_DiffuseDitherPalette, screenPos * _ScreenParams.xy / _DiffuseDitherPalette_ST.xy).r;
    palettePropOutput.g = (palettePropOutput.g * 2.0 - 1.0) * _DiffuseDitherStrength;
    palettePropOutput.g = palettePropOutput.g * 0.5 + 0.5;
    palettePropOutput.b = float(_EdgeLevel) / 128.0 * 0.5 + 0.5;
    int applyOutline = _ApplyOutline > 0 ? 1 : 0;
    palettePropOutput.a = PackFloatInt8bit(0.0, (applyOutline << 7), 256.0);

    shapePropOutput.r = _Priority * tex2D(_PriorityMap, input.uv * _PriorityMap_ST.xy + _PriorityMap_ST.zw).r;
    // shapePropOutput.g = _NormalBlendScale;
    shapePropOutput.b = _NormalEdgeThreshold;
    shapePropOutput.a = _AAScale / 255.0;

    outPhysical = physicalPropOutput;
    outShape = shapePropOutput;
    outPalette = palettePropOutput;
    outRimLight = _RimLightColor;
    outLightmapUV = float4(input.staticLightmapUV, input.dynamicLightmapUV);
}