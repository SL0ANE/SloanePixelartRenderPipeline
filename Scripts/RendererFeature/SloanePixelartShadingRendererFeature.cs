using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartShadingRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Shader m_DiffuseShadingShader;
        private ShaderBlitPass m_DiffuseShadingPass;

        public override void Create()
        {
            m_DiffuseShadingPass = new ShaderBlitPass("Diffuse Shading", m_DiffuseShadingShader, TargetBuffer.Albedo, TargetBuffer.Diffuse)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_DiffuseShadingPass);
        }
    }
}
