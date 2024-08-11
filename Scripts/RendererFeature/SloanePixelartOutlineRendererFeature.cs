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
        private ShaderBlitPass m_OutlinePass;

        public override void Create()
        {
            if(m_OutlintShader == null) return;

            m_OutlinePass = new ShaderBlitPass(m_OutlintShader, "Outline")
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

            var camera = renderingData.cameraData.camera;

            m_OutlinePass.SetSourceBuffer(renderer.cameraColorTargetHandle);
            m_OutlinePass.SetTargetBuffer(renderer.cameraColorTargetHandle);
        }
    }
}
