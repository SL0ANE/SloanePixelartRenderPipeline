Shader "Sloane/Pixelart/Unlit"
{
    Properties
    {
        _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _LocalUnitScale("Local Unit Scale", Int) = 1
    }
    SubShader
    {
        Tags {"RenderType" = "Opaque"}

        HLSLINCLUDE
        #define ALIGN_TO_PIXEL
        #define UNIT_SCALE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #pragma multi_compile_instancing

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        int _LocalUnitScale;
        CBUFFER_END

        #include "Includes/Common.hlsl"

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
                outNormal = float4(normalize(input.normalWS), 1.0);
            }

            #pragma vertex PixelartBaseVert
            #pragma fragment UnlitFrag
            
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
