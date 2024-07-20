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

            #include "../Includes/Math.hlsl"
            #include "../Includes/Blit.hlsl"
            #include "../Includes/Transform.hlsl"
            #include "../Includes/Lighting.hlsl"

            sampler2D _MainTex;
            sampler2D _NormalBuffer;
            sampler2D _DepthBuffer;

            float3 DiffuseShading(Light light, float3 normal, float level, float transmission)
            {
                float ndotl = dot(light.direction, normal);
                ndotl *= light.distanceAttenuation;
                ndotl = saturate(ndotl);
                ndotl = pow(ndotl, 1.0 / 2.2);
                
                ndotl = multiStep(ndotl, level, 0.0, 0.0);
                
                ndotl *= level;
                ndotl = round(ndotl);
                ndotl /= level;

                ndotl = lerp(transmission, 1.0, ndotl);

                float3 outputColor = ndotl * light.color;
                return outputColor;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;

                float sceneRawDepth = tex2D(_DepthBuffer, uv).r;

                float3 positionWS = GetWorldPositionWithDepth(uv, sceneRawDepth);
                float3 normalWS = tex2D(_NormalBuffer, uv).xyz;
                float3 outputColor = float3(0.0, 0.0, 0.0);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
                mainLight.distanceAttenuation = 1.0;
                outputColor += DiffuseShading(mainLight, normalWS, 3.0, 0.0);

                LIGHT_LOOP_BEGIN(_AdditionalLightCount)
                    Light light = GetAdditionalPerObjectLight(lightIndex, positionWS);
                    outputColor += DiffuseShading(light, normalWS, 3.0, 0.0);
                LIGHT_LOOP_END

                // return float4((positionWS + float3(0.0, 0.0, 11.0)) * 0.5 + 0.5, 1.0);
                return float4(outputColor, 1.0);
            }
            ENDHLSL
        }
    }
}
