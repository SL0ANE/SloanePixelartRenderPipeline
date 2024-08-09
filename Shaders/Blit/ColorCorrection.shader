Shader "Hidden/Sloane/Pixelart/ColorCorrection"
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

            #include "../Includes/Math.hlsl"
            #include "../Includes/Blit.hlsl"
            #include "../Includes/Lighting.hlsl"

            sampler2D _MainTex;
            Texture2D _Palette;
            SamplerState sampler_Palette_point_clamp_sampler;
            int _Resolution;

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                float res = float(_Resolution);

                float3 outputColor = tex2D(_MainTex, uv).rgb;
                outputColor = clamp(outputColor, 0.001, 0.999);

                float2 targetCoord = float2(((floor(outputColor.r * res) + 0.5) / res + floor(outputColor.b * res)) / res, 1.0 - floor(outputColor.g * res) / res);
                outputColor = _Palette.Sample(sampler_Palette_point_clamp_sampler, targetCoord).rgb;

                return float4(outputColor, 1.0);
            }
            ENDHLSL
        }
    }
}
