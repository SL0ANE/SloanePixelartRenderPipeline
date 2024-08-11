#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

#include "../Inputs/CameraParams.hlsl"
#include "../Inputs/BuffersResolver.hlsl"
#include "../Blit.hlsl"

sampler2D _MainTex;
sampler2D _ConnectivityResultBuffer;

float3 ApplyOutline(float2 uv)
{
    float3 baseColor = tex2D(_MainTex, uv).rgb;
    return baseColor * 0.5;
}

half4 OutlineFragment(Varyings input) : SV_Target
{
    GET_BLIT_UV
    GET_CONNECTIVITY

    float3 outputColor = float3(0.0, 0.0, 0.0);
    float weight = 0.0;

    if(connectedToRight < 1 && closerThanRight < 1)
    {
        outputColor += ApplyOutline(uv + float2(_ScreenParams.z - 1.0, 0.0));
        weight += 1.0;
    }

    if(connectedToLeft < 1 && closerThanLeft < 1)
    {
        outputColor += ApplyOutline(uv - float2(_ScreenParams.z - 1.0, 0.0));
        weight += 1.0;
    }

    if(connectedToUp < 1 && closerThanUp < 1)
    {
        outputColor += ApplyOutline(uv + float2(0.0, _ScreenParams.w - 1.0));
        weight += 1.0;
    }

    if(connectedToDown < 1 && closerThanDown < 1)
    {
        outputColor += ApplyOutline(uv - float2(0.0, _ScreenParams.w - 1.0));
        weight += 1.0;
    }

    if(weight > 0) outputColor /= weight;
    else outputColor = tex2D(_MainTex, uv).rgb;

    return float4(outputColor, 1.0);
}