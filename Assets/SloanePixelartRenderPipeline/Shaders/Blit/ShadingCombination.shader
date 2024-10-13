Shader "Hidden/Sloane/Pixelart/Blit/ShadingCombination"
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
