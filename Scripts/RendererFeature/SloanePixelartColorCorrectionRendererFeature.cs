using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartColorCorrectionRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Shader m_ColorCorrectionShader;
        [SerializeField]
        private Texture2D m_Palette;
        private ShaderBlitPass m_ColorCorrectionPass;
        public override void Create()
        {
            if(m_ColorCorrectionShader == null) return;

            m_ColorCorrectionPass = new ShaderBlitPass(m_ColorCorrectionShader, "Color Correction", BeforeBlit)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(m_ColorCorrectionPass == null) return;
            renderer.EnqueuePass(m_ColorCorrectionPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if(m_ColorCorrectionPass == null) return;

            m_ColorCorrectionPass.SetTargetBuffer(renderer.cameraColorTargetHandle);
            m_ColorCorrectionPass.SetSourceBuffer(renderer.cameraColorTargetHandle);
        }

        protected void BeforeBlit(CommandBuffer cmd, RenderingData renderingData)
        {
            cmd.SetGlobalInt(ShaderPropertyStorage.Resolution, m_Palette.height);
            cmd.SetGlobalTexture(ShaderPropertyStorage.Palette, m_Palette);
        }
    }
}
