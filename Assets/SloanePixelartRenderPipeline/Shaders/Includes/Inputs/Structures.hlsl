struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 normalWS : TEXCOORD0;
    float4 tangentWS : TEXCOORD1; 
    float2 uv : TEXCOORD2;
    float3 positionWS : TEXCOORD3;
    float3 positionVS : TEXCOORD4;
    float4 positionSS : TEXCOORD5;
    float2 positionSSO : TEXCOORD6;

    float2 staticLightmapUV   : TEXCOORD7;
    float2 dynamicLightmapUV  : TEXCOORD8;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};