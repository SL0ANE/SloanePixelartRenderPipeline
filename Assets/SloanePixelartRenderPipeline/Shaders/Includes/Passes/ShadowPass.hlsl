#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "../Inputs/Buffers.hlsl"
#include "../Inputs/Structures.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
    int _LocalUnitScale;
    int _MainLightLevel;
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(Props)
    UNITY_DEFINE_INSTANCED_PROP(float4x4, _SnapOffset)
UNITY_INSTANCING_BUFFER_END(Props)

float3 _LightDirection;

#include "CommonPass.hlsl"

Varyings ShadowVert(Attributes input)
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
    
    // align to camera, transform to light.
    output.positionWS = mul(PIXELART_CAMERA_MATRIX_I_V, float4(output.positionVS, 1.0));

#if _CASTING_PUNCTUAL_LIGHT_SHADOW
    float3 lightDirectionWS = normalize(_LightPosition - output.positionWS);
#else
    float3 lightDirectionWS = _LightDirection;
#endif

    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.positionCS = TransformWorldToHClip(ApplyShadowBias(output.positionWS, output.normalWS, lightDirectionWS));
    
    #if UNITY_REVERSED_Z
        output.positionCS.z = min(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
        output.positionCS.z = max(output.positionCS.z, output.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif

    output.uv = input.uv;

    return output;
}

half4 ShadowFrag(Varyings input) : SV_Target 
{
    return float4(0.0, 0.0, 0.0, 0.0);
}