#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

#include "../Inputs/CameraParams.hlsl"
#include "../Inputs/BuffersResolver.hlsl"
#include "../Math.hlsl"
#include "../Blit.hlsl"
#include "../Transform.hlsl"

uint _AdditionalLightCount;
float _ConnectivityAntialiasingThreshold;

sampler2D _AlbedoBuffer;
sampler2D _NormalBuffer;
sampler2D _DepthBuffer;
sampler2D _PhysicalPropertyBuffer;
sampler2D _ShapePropertyBuffer;
sampler2D _PalettePropertyBuffer;
sampler2D _ConnectivityResultBuffer;

sampler2D _DiffuseBuffer;
sampler2D _SpecularBuffer;

float3 DiffuseShading(Light light, float3 normal, float connect, float level, float transmission)
{
    float ndotl = dot(light.direction, normal);
    ndotl *= light.distanceAttenuation * light.shadowAttenuation;
    ndotl = saturate(ndotl);
    

    if(level > 0.0)
    {
        ndotl = pow(ndotl, 1.0 / 2.2);
    
        ndotl = multiStep(ndotl, level, 0.0, 0.0);
        float singleLevel = 1.0 / (level - 1.0);
        if(ndotl > singleLevel && connect < _ConnectivityAntialiasingThreshold) ndotl -= singleLevel;

        ndotl = lerp(transmission, 1.0, ndotl);
    }

    float3 outputColor = ndotl * light.color;
    return outputColor;
}

half4 DiffuseFragment(Varyings input) : SV_Target
{
    GET_BLIT_UV
    GET_POSITION
    GET_ALBEDO
    GET_CONNECTIVITY
    GET_PROP
    GET_NORMAL
    
    float3 outputColor = float3(0.0, 0.0, 0.0);
    
    float mainLightLevel = paletteProp.r * 255.0;

    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				float4 shadowCoord = GetShadowCoord(vertexInput);

    Light mainLight = GetMainLight(shadowCoord);
    mainLight.distanceAttenuation = 1.0;
    outputColor += DiffuseShading(mainLight, normalWS, connectInfo.g, mainLightLevel, 0.0);

    LIGHT_LOOP_BEGIN(_AdditionalLightCount)
        Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);
        outputColor += DiffuseShading(light, normalWS, connectInfo.g, 3.0, 0.0);
    LIGHT_LOOP_END

    outputColor *= albedo.rgb;

    return float4(outputColor, 1.0);
}

half3 SpecularShading(Light light, float3 normal, float3 viewDir, float3 specular, float smoothness, float expSmoothness, float level)
{
    float3 halfVec = SafeNormalize(float3(light.direction) + float3(viewDir));

    half NdotH = half(saturate(dot(normal, halfVec)));
    float modifier = pow(NdotH, expSmoothness) * light.distanceAttenuation * light.shadowAttenuation;
    modifier = multiStep(modifier, level, 0.0, 0.0);
    
    return light.color * specular * modifier * smoothness;
}

float3 GetViewDir()
{
    return float3(PIXELART_CAMERA_MATRIX_I_V._m02, PIXELART_CAMERA_MATRIX_I_V._m12, PIXELART_CAMERA_MATRIX_I_V._m22);
}

half4 SpecularFragment(Varyings input) : SV_Target
{
    GET_BLIT_UV
    GET_POSITION
    GET_ALBEDO
    GET_CONNECTIVITY
    GET_PROP
    GET_NORMAL

    float smoothness = physicalProp.r;
    float expSmoothness = exp2(10.0 * smoothness + 1.0);
    float metallic = physicalProp.g;
    float3 specular = lerp(float3(1.0, 1.0, 1.0), albedo, metallic);

    float3 viewDir = GetViewDir();
    
    float3 outputColor = float3(0.0, 0.0, 0.0);
    
    float mainLightLevel = paletteProp.r * 255.0;

    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				float4 shadowCoord = GetShadowCoord(vertexInput);

    Light mainLight = GetMainLight(shadowCoord);
    mainLight.distanceAttenuation = 1.0;
    outputColor += SpecularShading(mainLight, normalWS, viewDir, specular, smoothness, expSmoothness, 2.0);

    LIGHT_LOOP_BEGIN(_AdditionalLightCount)
        Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);
        outputColor += SpecularShading(light, normalWS, viewDir, specular, smoothness, expSmoothness, 2.0);
    LIGHT_LOOP_END

    return float4(outputColor, 1.0);
}

half4 CombineFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    float2 uv = input.texcoord;

    float4 diffuse = tex2D(_DiffuseBuffer, uv);
    float4 specular = tex2D(_SpecularBuffer, uv);

    return float4(diffuse.rgb + specular.rgb, 1.0);
}