#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

#include "../Inputs/CameraParams.hlsl"
#include "../Inputs/BuffersResolver.hlsl"
#include "../Math/Step.hlsl"
#include "../Math/FibonacciSphere.hlsl"
#include "../Blit.hlsl"
#include "../Transform.hlsl"
#include "../RimLight.hlsl"

uint _AdditionalLightCount;
float _ConnectivityAntialiasingThreshold;

sampler2D _UVBuffer;
sampler2D _AlbedoBuffer;
sampler2D _Normal0Buffer;
sampler2D _Normal1Buffer;
sampler2D _DepthBuffer;
sampler2D _PhysicalPropertyBuffer;
sampler2D _ShapePropertyBuffer;
sampler2D _PalettePropertyBuffer;
sampler2D _RimLightPropertyBuffer;
sampler2D _LightmapUVBuffer;

sampler2D _ConnectivityResultBuffer;

sampler2D _DiffuseBuffer;
sampler2D _SpecularBuffer;
sampler2D _RimLightBuffer;
sampler2D _GlobalIlluminationBuffer;

float _SamplingScale;

float3 DiffuseShading(Light light, float3 normal, float connect, float normalDiff, float normalEdgeThreshold, float normalEdgeLevel, float level, float offset, float transmission, float applyAA = 1.0)
{
    float ndotl = dot(light.direction, normal);
    ndotl *= light.distanceAttenuation * light.shadowAttenuation;
    ndotl = saturate(ndotl);
    

    if(level > 0.0)
    {
        float singleLevel = 1.0 / (level - 1.0);

        ndotl = pow(ndotl, 1.0 / 2.2);
        ndotl = saturate(ndotl);
        ndotl += singleLevel * offset;

        ndotl = multiStep(ndotl, level, 0.0, 0.0);
        if((ndotl > singleLevel && connect < _ConnectivityAntialiasingThreshold)) ndotl -= applyAA * singleLevel;
        if((normalDiff > normalEdgeThreshold)) ndotl += singleLevel * normalEdgeLevel;

        ndotl = clamp(ndotl, 0.0, 1.0 + singleLevel);

        ndotl = lerp(transmission, 1.0, ndotl);
    }

    float3 outputColor = ndotl * light.color;
    return outputColor;
}

half4 DiffuseFragment(Varyings input) : SV_Target
{
    GET_BLIT_UV
    GET_POSITION
    GET_UV_WITH_PRIORITY
    GET_ALBEDO
    GET_CONNECTIVITY
    GET_PALETTE_PROP
    GET_SHAPE_PROP
    GET_PHYSICAL_PROP
    GET_RIMLIGHT_PROP
    GET_NORMAL1

    // normalWS = FibonacciSphereMap(normalWS, 128);
    
    float3 outputColor = float3(0.0, 0.0, 0.0);
    float metallic = physicalProp.g;

    float mainLightLevel = paletteProp.r * 255.0;

    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				float4 shadowCoord = GetShadowCoord(vertexInput);

    float ditherOffset = paletteProp.g * 2.0 - 1.0;
    float applyAA = 1.0;
    float3 rimLightInfo = tex2D(_RimLightBuffer, uv).rgb;
    if(rimLightInfo.r > 0.0 || rimLightInfo.g > 0.0 || rimLightInfo.g > 0.0) applyAA = 0;
    float normalEdgeLevel = (paletteProp.b * 2.0 - 1.0) * 128.0;

    applyAA *= shapeProp.a * 255.0;

    Light mainLight = GetMainLight(shadowCoord);
    outputColor += DiffuseShading(mainLight, normalWS, connectInfo.g, connectInfo.b, shapeProp.b, normalEdgeLevel, mainLightLevel, ditherOffset, 0.0, applyAA);

    LIGHT_LOOP_BEGIN(_AdditionalLightCount)
        Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);
        outputColor += DiffuseShading(light, normalWS, connectInfo.g, connectInfo.b, shapeProp.b, normalEdgeLevel * 0.5, 3.0, ditherOffset, 0.0, applyAA);
    LIGHT_LOOP_END

    outputColor *= albedo.rgb;
    outputColor = outputColor * (1.0 - metallic);

    return float4(outputColor, 1.0);
}

float3 GetViewDir()
{
    return float3(PIXELART_CAMERA_MATRIX_I_V._m02, PIXELART_CAMERA_MATRIX_I_V._m12, PIXELART_CAMERA_MATRIX_I_V._m22);
}

half3 SpecularShading(Light light, float3 normal, float3 viewDir, float3 specular, float smoothness, float expSmoothness, float level)
{
    float3 halfVec = SafeNormalize(float3(light.direction) + float3(viewDir));

    half NdotH = half(saturate(dot(normal, halfVec)));
    float modifier = pow(NdotH, expSmoothness) * light.distanceAttenuation * light.shadowAttenuation;
    modifier = multiStep(modifier, level, 0.0, 0.0);
    
    return light.color * specular * modifier * smoothness;
}

half4 SpecularFragment(Varyings input) : SV_Target
{
    GET_BLIT_UV
    GET_POSITION
    GET_UV_WITH_PRIORITY
    GET_ALBEDO
    GET_CONNECTIVITY
    GET_PALETTE_PROP
    GET_SHAPE_PROP
    GET_PHYSICAL_PROP
    GET_RIMLIGHT_PROP
    GET_NORMAL1

    float smoothness = physicalProp.r;
    float expSmoothness = exp2(5.0 * smoothness + 1.0);
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
    outputColor += SpecularShading(mainLight, normalWS, viewDir, specular, smoothness, expSmoothness, 2.0);

    LIGHT_LOOP_BEGIN(_AdditionalLightCount)
        Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);
        outputColor += SpecularShading(light, normalWS, viewDir, specular, smoothness, expSmoothness, 2.0);
    LIGHT_LOOP_END

    return float4(outputColor, 1.0);
}

half3 GlobalIlluminationShading(BRDFData brdfData, half3 bakedGI, half occlusion, half3 normalWS, half3 viewDirectionWS, float levelBias)
{
    const BRDFData noClearCoat = (BRDFData)0;
    
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half NoV = multiStep(saturate(dot(normalWS, viewDirectionWS)), 2.0, 0.0, levelBias);
    half fresnelTerm = lerp(Pow4(1.0 - NoV), 0.0, brdfData.reflectivity);

    half3 indirectDiffuse = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, brdfData.perceptualRoughness, 1.0h);

    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);
    return color * occlusion;
}

half4 GlobalIlluminationFragment(Varyings input) : SV_Target
{
    GET_BLIT_UV
    GET_POSITION
    GET_UV_WITH_PRIORITY
    GET_ALBEDO
    GET_CONNECTIVITY
    GET_PALETTE_PROP
    GET_SHAPE_PROP
    GET_PHYSICAL_PROP
    GET_RIMLIGHT_PROP
    GET_NORMAL1
    GET_LIGHTMAP_UV

    // normalWS = FibonacciSphereMap(normalWS, 128);

    float smoothness = physicalProp.r;
    float expSmoothness = exp2(10.0 * smoothness + 1.0);
    float metallic = physicalProp.g;
    float3 specular = lerp(float3(0.0, 0.0, 0.0), albedo, metallic);
    float3 viewDir = GetViewDir();

    float oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
    float reflectivity = 1.0 - oneMinusReflectivity;
    BRDFData brdfData = (BRDFData)0;
    brdfData.albedo = albedo;
    brdfData.diffuse = albedo * oneMinusReflectivity;
    brdfData.specular = specular;
    brdfData.reflectivity = reflectivity;

    brdfData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
    brdfData.roughness           = max(PerceptualRoughnessToRoughness(brdfData.perceptualRoughness), HALF_MIN_SQRT);
    brdfData.roughness2          = max(brdfData.roughness * brdfData.roughness, HALF_MIN);
    brdfData.grazingTerm         = clamp(smoothness + reflectivity, 0.0, 1.0);
    brdfData.normalizationTerm   = brdfData.roughness * 4.0h + 2.0h;
    brdfData.roughness2MinusOne  = brdfData.roughness2 - 1.0h;

    int applyAA = 1;
    float3 rimLightInfo = tex2D(_RimLightBuffer, uv).rgb;
    if(rimLightInfo.r > 0.0 || rimLightInfo.g > 0.0 || rimLightInfo.g > 0.0) applyAA = 0;

    float3 SH;
    OUTPUT_SH(normalWS, SH);
    float3 bakedGI = SAMPLE_GI(lightmapUV, SH, normalWS);
    float3 outputColor = GlobalIlluminationShading(brdfData, bakedGI, 1.0, normalWS, viewDir, (connectInfo.g < _ConnectivityAntialiasingThreshold && applyAA == 1.0) ? 1.0 : 0.0);
    
    return float4(outputColor, 1.0);
}

half4 CombineFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    float2 uv = input.texcoord;

    float4 diffuse = tex2D(_DiffuseBuffer, uv);
    clip(diffuse.a - 1);
    float4 specular = tex2D(_SpecularBuffer, uv);
    float4 globalIllumination = tex2D(_GlobalIlluminationBuffer, uv);
    float4 rimLight = tex2D(_RimLightBuffer, uv);

    return float4(diffuse.rgb + specular.rgb + globalIllumination.rgb + rimLight.rgb, 1.0);
}

half4 RimLightFragment(Varyings input) : SV_Target
{
    GET_BLIT_UV
    GET_POSITION
    GET_UV_WITH_PRIORITY
    GET_ALBEDO
    GET_CONNECTIVITY
    GET_PALETTE_PROP
    GET_SHAPE_PROP
    GET_PHYSICAL_PROP
    GET_RIMLIGHT_PROP
    GET_NORMAL1
    
    float3 outputColor = float3(0.0, 0.0, 0.0);
    float mainLightLevel = paletteProp.r * 255.0;
    float metallic = physicalProp.g;
    float3 specular = lerp(float3(1.0, 1.0, 1.0), albedo, metallic);

    VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				float4 shadowCoord = GetShadowCoord(vertexInput);

    /* Light mainLight = GetMainLight(shadowCoord);
    outputColor += rimLightProp.rgb * rimLightProp.a * RimLightShading(mainLight, normalWS, specular, 2.0, connectedToRight | !closerThanRight, connectedToLeft | !closerThanLeft, connectedToUp | !closerThanUp, connectedToDown | !closerThanDown);

    LIGHT_LOOP_BEGIN(_AdditionalLightCount)
        Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);
        outputColor += rimLightProp.rgb * rimLightProp.a * RimLightShading(light, normalWS, specular, 2.0, connectedToRight | !closerThanRight, connectedToLeft | !closerThanLeft, connectedToUp | !closerThanUp, connectedToDown | !closerThanDown);
    LIGHT_LOOP_END */

    for(int i = 0; i < _RimLightsCount; i++)
    {
        RimLight rimLight = _RimLights[i];
        outputColor += rimLightProp.rgb * rimLightProp.a * RimLightShading(rimLight.direction, rimLight.color, 1.0, normalWS, specular, 2.0, connectedToRight | !closerThanRight, connectedToLeft | !closerThanLeft, connectedToUp | !closerThanUp, connectedToDown | !closerThanDown);
    }

    return float4(outputColor, 1.0);
}