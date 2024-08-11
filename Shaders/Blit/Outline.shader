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
            #pragma vertex Vert
            #pragma fragment OutlineFragment

            ENDHLSL
        }
    }
}
