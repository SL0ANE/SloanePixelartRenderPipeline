#define GET_POSITION \
float sceneRawDepth = tex2D(_DepthBuffer, uv).r; \
\
float3 positionWS = GetWorldPositionWithDepth(uv, sceneRawDepth); \
float4 positionCS = mul(PIXELART_CAMERA_MATRIX_VP, float4(positionWS, 1.0));


#define GET_ALBEDO \
float4 albedo = tex2D(_AlbedoBuffer, uv);


#define GET_CONNECTIVITY \
float4 connectInfo = tex2D(_ConnectivityResultBuffer, uv); \
int connectData; \
float fakeFloot = 0.0; \
UnpackFloatInt8bit(connectInfo.a, 256.0, fakeFloot, connectData); \
\
int connectedToRight = (connectData & (1 << 7)) > 0 ? 1 : 0; \
int connectedToLeft = (connectData & (1 << 6)) > 0 ? 1 : 0; \
int connectedToUp = (connectData & (1 << 5)) > 0 ? 1 : 0; \
int connectedToDown = (connectData & (1 << 4)) > 0 ? 1 : 0; \
\
int closerThanRight = (connectData & (1 << 3)) > 0 ? 1 : 0; \
int closerThanLeft = (connectData & (1 << 2)) > 0 ? 1 : 0; \
int closerThanUp = (connectData & (1 << 1)) > 0 ? 1 : 0; \
int closerThanDown = (connectData & (1 << 0)) > 0 ? 1 : 0;


#define GET_PROP \
float4 paletteProp = tex2D(_PalettePropertyBuffer, uv); \
float4 shapeProp = tex2D(_ShapePropertyBuffer, uv); \
float4 physicalProp = tex2D(_PhysicalPropertyBuffer, uv);

#define GET_LIGHTMAP_UV \
float4 UVInfo = tex2D(_LightmapUVBuffer, uv); \
float2 staticLightmapUV = UVInfo.xy; \
float2 dynamicLightmapUV = UVInfo.zw; \

#define GET_NORMAL \
float3 normalWS = tex2D(_NormalBuffer, uv).xyz; \
float3 blendNormalWS = normalWS; \
if(connectedToRight > 0) blendNormalWS += tex2D(_NormalBuffer, uv + float2(_ScreenParams.z - 1.0, 0.0)).xyz; \
if(connectedToLeft > 0) blendNormalWS += tex2D(_NormalBuffer, uv - float2(_ScreenParams.z - 1.0, 0.0)).xyz; \
if(connectedToUp > 0) blendNormalWS += tex2D(_NormalBuffer, uv + float2(0.0, _ScreenParams.w - 1.0)).xyz; \
if(connectedToDown > 0) blendNormalWS += tex2D(_NormalBuffer, uv - float2(0.0, _ScreenParams.w - 1.0)).xyz;