Shader "Hidden/Sloane/Pixelart/Blit/Outline"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        HLSLINCLUDE
        #include "../Includes/Passes/OutlinePass.hlsl"
        ENDHLSL

        Pass
        {
            Name "Outline"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma multi_compile _ _OUTLINE_SOLID_COLOR
            #pragma vertex Vert
            #pragma fragment OutlineFragment

            ENDHLSL
        }
    }
}
