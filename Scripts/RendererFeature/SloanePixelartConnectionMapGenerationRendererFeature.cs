using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartConnectionMapGenerationRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private float m_DepthCurvThreshold = 0.1f;
        [SerializeField]
        private float m_DepthDiffThreshold = 0.1f;
        [SerializeField]
        private ComputeShader m_ConnectionCheckShader;
        private ComputeShaderPass m_GenerationPass;

        public override void Create()
        {
            m_GenerationPass = new ComputeShaderPass(m_ConnectionCheckShader, "Main", "GenerateConnectionMap", 8, 8, BeforeDispatch)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            renderer.EnqueuePass(m_GenerationPass);
            m_GenerationPass.ClearSourceBuffers();
            m_GenerationPass.AddSourceBuffer("_DepthBuffer", pixelartCamera.GetBuffer(TargetBuffer.Depth));
            m_GenerationPass.SetTargetBuffer("_ConnectionMap", pixelartCamera.GetBuffer(TargetBuffer.Connection));
        }

        private void BeforeDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            cmd.SetComputeFloatParam(computeShader, ShaderPropertyStorage.DepthCurvThreshold, m_DepthCurvThreshold);
            cmd.SetComputeFloatParam(computeShader, ShaderPropertyStorage.DepthDiffThreshold, m_DepthDiffThreshold);
            cmd.SetComputeIntParam(computeShader, ShaderPropertyStorage.SamplingScale, pixelartCamera.DownSamplingScale);
        }
    }
}
