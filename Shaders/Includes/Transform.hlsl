float3 GetWorldPositionWithDepth(float2 uv, float sceneRawDepth)
{
    float3 worldPos;
    #if defined(UNITY_REVERSED_Z)
        sceneRawDepth = 1 - sceneRawDepth;
    #endif

    if(unity_OrthoParams.w)
    {
        float sceneDepthVS = lerp(_ProjectionParams.y, _ProjectionParams.z, sceneRawDepth);
        float2 viewRayEndPosVS_xy = float2(unity_OrthoParams.xy * uv);
        float3 posVSOrtho = float3(-viewRayEndPosVS_xy, -sceneDepthVS);

        worldPos = mul(unity_CameraToWorld, float4(posVSOrtho, 1)).xyz;
    }
    else
    {
        float4 ndc = float4(uv * 2.0 - 1.0, sceneRawDepth * 2.0 - 1.0, 1);
        float4 worldPos = mul(UNITY_MATRIX_I_VP, ndc);
        worldPos /= worldPos.w;
    }

    return worldPos;
}