Shader "Hidden/Sloane/Pixelart/Blit/Shading"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        HLSLINCLUDE
        #include "../Includes/Passes/ShadingPass.hlsl"
        ENDHLSL

        Pass
        {
            Name "Diffuse"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment DiffuseFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _SHADOWS_SOFT

            ENDHLSL
        }

        Pass
        {
            Name "Specular"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment SpecularFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _SHADOWS_SOFT

            ENDHLSL
        }

        Pass
        {
            Name "GlobalIllumination"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment GlobalIlluminationFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _SHADOWS_SOFT

            ENDHLSL
        }

        Pass
        {
            Name "RimLight"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment RimLightFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _SHADOWS_SOFT

            ENDHLSL
        }

        Pass
        {
            Name "ShadingCombination"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment CombineFragment

            ENDHLSL
        }
    }
}
