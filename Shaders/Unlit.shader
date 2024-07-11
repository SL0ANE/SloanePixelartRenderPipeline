Shader "Sloane/Pixelart/Unlit"
{
    Properties
    {
        _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque"}

        HLSLINCLUDE
        #define ALIGN_TO_PIXEL
        #include "Includes/Common.hlsl"
        #pragma multi_compile_instancing

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        CBUFFER_END

        ENDHLSL

        Pass
        {
            Tags
            {
                "LightMode" = "PixelartOpaque"
            }

            HLSLPROGRAM

            void UnlitFrag(Varyings input, out float4 outAlbedo : BUFFER_ALBEDO, out float4 outNormal: BUFFER_NORMAL)
            {
                outAlbedo = _BaseColor;
                outNormal = float4(input.normalWS, 1.0);
            }

            #pragma vertex PixelartBaseVert
            #pragma fragment UnlitFrag
            
            ENDHLSL
        }
    }
}
