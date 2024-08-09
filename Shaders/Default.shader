Shader "Sloane/Pixelart/Default"
{
    Properties
    {
        _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _LocalUnitScale("Local Unit Scale", Int) = 1
        _MainLightLevel("Main Light Level", Int) = 2
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque"}

        HLSLINCLUDE
        #define ALIGN_TO_PIXEL
        #define UNIT_SCALE

        ENDHLSL

        Pass
        {
            Name "PixelartOpaque"

            Tags
            {
                "LightMode" = "PixelartOpaque"
            }

            HLSLPROGRAM

            #pragma multi_compile_instancing
            #include "Includes/Passes/DefaultPass.hlsl"

            #pragma vertex PixelartBaseVert
            #pragma fragment DefaultFrag
            
            ENDHLSL
        }

        Pass
        {
            Name "PixelartPreview"

            Tags
            {
                "LightMode" = "PixelartPreview"
            }

            // -------------------------------------
            // Render State Commands
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAMODULATE_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"

            Tags
            {
                "LightMode" = "ShadowCaster"
            }

			ZWrite On
            Blend One Zero
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM

            #pragma multi_compile_instancing
            #include "Includes/Passes/ShadowPass.hlsl"

            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            
            ENDHLSL
        }
    }
}
