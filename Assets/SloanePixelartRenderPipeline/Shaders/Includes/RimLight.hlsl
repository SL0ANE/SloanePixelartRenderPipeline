struct RimLight
{
    float3 direction;
    float3 color;
};

StructuredBuffer<RimLight> _RimLights;
uint _RimLightsCount;

half3 RimLightShading(float3 direction, float3 color, float factor, float3 normal, float3 specular, float level, int connectedToRight, int connectedToLeft, int connectedToUp, int connectedToDown)
{
    float ndotl = dot(direction, normal);
    ndotl *= factor;
    ndotl = saturate(ndotl);

    float2 screenSpaceLightDir = normalize(TransformWorldToViewDir(direction).xy);
    float modifier = (1.0 - float(connectedToRight)) * clamp(screenSpaceLightDir.x, 0.0, 1.0)
                   + (1.0 - float(connectedToLeft)) * clamp(-screenSpaceLightDir.x, 0.0, 1.0)
                   + (1.0 - float(connectedToUp)) * clamp(screenSpaceLightDir.y, 0.0, 1.0)
                   + (1.0 - float(connectedToDown)) * clamp(-screenSpaceLightDir.y, 0.0, 1.0);

    modifier = multiStep(modifier * ndotl, level, 0.0, 0.0);

    return color * specular * modifier;
}

half3 RimLightShading(Light light, float3 normal, float3 specular, float level, int connectedToRight, int connectedToLeft, int connectedToUp, int connectedToDown)
{
    float modifier = light.distanceAttenuation * light.shadowAttenuation;

    return RimLightShading(light.direction, light.color, modifier, normal, specular, level, connectedToRight, connectedToLeft, connectedToUp, connectedToDown);
}