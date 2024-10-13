using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartCombineShadingResultsRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Shader m_ShadingShader;
        private ShaderBlitPass m_CombinationPass;

        public override void Create()
        {
            if (m_ShadingShader == null) return;

            m_CombinationPass = new ShaderBlitPass(m_ShadingShader, "Shading Combination", null, null, 0)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_ShadingShader == null) return;
            renderer.EnqueuePass(m_CombinationPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (m_ShadingShader == null) return;

            var camera = renderingData.cameraData.camera;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);
            var albedoBuffer = pixelartCamera.GetBuffer(TargetBuffer.Diffuse);

            m_CombinationPass.SetSourceBuffer(albedoBuffer);
            m_CombinationPass.SetTargetBuffer(renderer.cameraColorTargetHandle);
        }
    }
}