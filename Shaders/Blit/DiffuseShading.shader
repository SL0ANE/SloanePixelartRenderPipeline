Shader "Hidden/Sloane/Pixelart/DiffuseShading"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            Name "Blit"
            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _SHADOWS_SOFT

            #include "../Includes/Inputs/CameraParams.hlsl"
            #include "../Includes/Math.hlsl"
            #include "../Includes/Blit.hlsl"
            #include "../Includes/Transform.hlsl"
            #include "../Includes/Lighting.hlsl"

            sampler2D _AlbedoBuffer;
            sampler2D _NormalBuffer;
            sampler2D _DepthBuffer;
            sampler2D _PhysicalPropertyBuffer;
            sampler2D _PalettePropertyBuffer;
            sampler2D _ConnectivityResultBuffer;

            float3 DiffuseShading(Light light, float3 normal, float connect, float level, float transmission)
            {
                float ndotl = dot(light.direction, normal);
                ndotl *= light.distanceAttenuation;
                ndotl = saturate(ndotl);
                

                if(level > 0.0)
                {
                    ndotl = pow(ndotl * light.shadowAttenuation, 1.0 / 2.2);
                
                    ndotl = multiStep(ndotl, level, 0.0, 0.0);
                    float singleLevel = 1.0 / (level - 1.0);
                    if(ndotl > singleLevel && connect < _ConnectivityAntialiasingThreshold) ndotl -= singleLevel;

                    ndotl = lerp(transmission, 1.0, ndotl);
                }

                float3 outputColor = ndotl * light.color;
                return outputColor;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                float sceneRawDepth = tex2D(_DepthBuffer, uv).r;

                float3 positionWS = GetWorldPositionWithDepth(uv, sceneRawDepth);
                float4 positionCS = mul(PIXELART_CAMERA_MATRIX_VP, float4(positionWS, 1.0));
                float3 normalWS = tex2D(_NormalBuffer, uv).xyz;
                float connect = tex2D(_ConnectivityResultBuffer, uv).g;
                float3 outputColor = float3(0.0, 0.0, 0.0);

                float4 paletteProp = tex2D(_PalettePropertyBuffer, uv);
                float mainLightLevel = paletteProp.r * 255.0;

                VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				float4 shadowCoord = GetShadowCoord(vertexInput);

                Light mainLight = GetMainLight(shadowCoord);
                mainLight.distanceAttenuation = 1.0;
                outputColor += DiffuseShading(mainLight, normalWS, connect, mainLightLevel, 0.0);

                LIGHT_LOOP_BEGIN(_AdditionalLightCount)
                    Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);
                    outputColor += DiffuseShading(light, normalWS, connect, 3.0, 0.0);
                LIGHT_LOOP_END

                outputColor *= tex2D(_AlbedoBuffer, uv).rgb;

                // return float4((positionWS / 256), 1.0);
                return float4(outputColor, 1.0);
            }
            ENDHLSL
        }
    }
}
