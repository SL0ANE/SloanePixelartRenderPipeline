using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartOutlineRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Shader m_OutlintShader;
        [SerializeField]
        private bool m_SolidColor = false;
        [SerializeField]
        private Color m_OutlineColor = new Color(1.0f, 1.0f, 1.0f);
        [SerializeField]
        private ShaderBlitPass m_OutlinePass;
        private GlobalKeyword m_SolidColorKeyWord;

        public override void Create()
        {
            if(m_OutlintShader == null) return;

            m_SolidColorKeyWord = GlobalKeyword.Create("_OUTLINE_SOLID_COLOR");

            m_OutlinePass = new ShaderBlitPass(m_OutlintShader, "Outline", BeforeBlit)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(m_OutlintShader == null) return;
            
            renderer.EnqueuePass(m_OutlinePass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if(m_OutlintShader == null) return;

            m_OutlinePass.SetSourceBuffer(renderer.cameraColorTargetHandle);
            m_OutlinePass.SetTargetBuffer(renderer.cameraColorTargetHandle);
        }

        protected void BeforeBlit(CommandBuffer cmd, RenderingData renderingData)
        {
            if(m_SolidColor) cmd.EnableKeyword(m_SolidColorKeyWord);
            else cmd.DisableKeyword(m_SolidColorKeyWord);
            cmd.SetGlobalColor(ShaderPropertyStorage.OutlineColor, m_OutlineColor);
        }
    }
}
