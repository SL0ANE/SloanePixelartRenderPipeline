#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

#include "../Inputs/CameraParams.hlsl"
#include "../Inputs/BuffersResolver.hlsl"
#include "../Blit.hlsl"

sampler2D _MainTex;
sampler2D _DiffuseBuffer;
sampler2D _ConnectivityResultBuffer;
sampler2D _PalettePropertyBuffer;
float4 _OutlineColor;
float _SamplingScale;

float3 ApplyOutline(float2 uv)
{
#ifdef _OUTLINE_SOLID_COLOR
    return _OutlineColor.rgb;
#else
    float3 baseColor = tex2D(_DiffuseBuffer, uv).rgb;
    return baseColor * _OutlineColor.rgb;
#endif
}

half4 OutlineFragment(Varyings input) : SV_Target
{
    GET_BLIT_UV
    GET_CONNECTIVITY

    float3 outputColor = float3(0.0, 0.0, 0.0);
    float weight = 0.0;
    float2 uvCache = uv;

    if(connectedToRight < 1 && closerThanRight < 1)
    {
        uv = uvCache + float2(_ScreenParams.z - 1.0, 0.0) * _SamplingScale;
        GET_PALETTE_PROP
        if(applyOutline > 0)
        {
            outputColor += ApplyOutline(uv);
            weight += 1.0;
        }
    }
    
    if(connectedToLeft < 1 && closerThanLeft < 1)
    {
        uv = uvCache - float2(_ScreenParams.z - 1.0, 0.0) * _SamplingScale;
        GET_PALETTE_PROP
        if(applyOutline > 0)
        {
            outputColor += ApplyOutline(uv);
            weight += 1.0;
        }
    }

    if(connectedToUp < 1 && closerThanUp < 1)
    {
        uv = uvCache + float2(0.0, _ScreenParams.w - 1.0) * _SamplingScale;
        GET_PALETTE_PROP
        if(applyOutline > 0)
        {
            outputColor += ApplyOutline(uv);
            weight += 1.0;
        }
    }

    if(connectedToDown < 1 && closerThanDown < 1)
    {
        uv = uvCache - float2(0.0, _ScreenParams.w - 1.0) * _SamplingScale;
        GET_PALETTE_PROP
        if(applyOutline > 0)
        {
            outputColor += ApplyOutline(uv);
            weight += 1.0;
        }
    }

    uv = uvCache;

    if(weight > 0) outputColor /= weight;
    else outputColor = tex2D(_MainTex, uv).rgb;

    return float4(outputColor, 1.0);
}