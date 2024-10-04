Texture2D<float> _DepthBuffer;
Texture2D<float4> _NormalBuffer;
float _Threshold;

#define LINEAR_DEPTH(rawDepth) ((unity_OrthoParams.w == 0)?LinearEyeDepth(rawDepth, _ZBufferParams):lerp(_ProjectionParams.y,_ProjectionParams.z,rawDepth))

void CheckConnectedDepthNormal(int2 center, int2 target, inout uint connected, inout uint closer, int step)
{
    if(target.x >= BUFFER_WIDTH || target.y >= BUFFER_HEIGHT || target.x < 0 || target.y < 0)
    {
        connected = 1;
        closer = 1;
        return;
    }

    float centerDepth = _DepthBuffer[center];
    float targetDepth = _DepthBuffer[target];

    #if defined(UNITY_REVERSED_Z)
    centerDepth = 1.0 - centerDepth;
    targetDepth = 1.0 - targetDepth;
    #endif

    centerDepth = LINEAR_DEPTH(centerDepth);
    targetDepth = LINEAR_DEPTH(targetDepth);

    closer = centerDepth < targetDepth ? 1 : 0;
    connected = 1;

    int2 prev = center;
    int2 next;
    for(int i = 1; i <= step; i++)
    {
        next = (target - center) * i / step + center;

        float2 centerUV = float2(float(prev.x) / float(BUFFER_WIDTH), float(prev.y) / float(BUFFER_HEIGHT));
        float3 centerPos = GetViewPositionWithDepth(centerUV, _DepthBuffer[prev]);

        float2 targetUV = float2(float(next.x) / float(BUFFER_WIDTH), float(next.y) / float(BUFFER_HEIGHT));
        float3 targetPos = GetViewPositionWithDepth(targetUV, _DepthBuffer[next]);

        float3 normalCenter = _NormalBuffer[prev].xyz;
        normalCenter = TransformWorldToViewDir(normalCenter);

        float2 pixelSpacing = (centerPos.xy - targetPos.xy);
        float dz_x = -normalCenter.x * pixelSpacing.x / normalCenter.z;
        float dz_y = -normalCenter.y * pixelSpacing.y / normalCenter.z;

        float predictDepth = centerDepth + dz_x + dz_y;

        float resultCenter = abs(predictDepth - targetDepth);

        float3 normalTarget = _NormalBuffer[next].xyz;
        normalTarget = TransformWorldToViewDir(normalTarget);

        dz_x = -normalTarget.x * pixelSpacing.x / normalTarget.z;
        dz_y = -normalTarget.y * pixelSpacing.y / normalTarget.z;

        predictDepth = centerDepth + dz_x + dz_y;

        float resultTarget = abs(predictDepth - targetDepth);

        if(dot(normalTarget, normalTarget) < 0.5 && dot(normalCenter, normalCenter) >= 0.5) connected = 0;
        else if(dot(normalTarget, normalTarget) >= 0.5 && dot(normalCenter, normalCenter) < 0.5) connected = 0;
        else if(resultCenter > _Threshold && resultTarget > _Threshold) connected = 0;

        if(connected == 0) return;
    }
}