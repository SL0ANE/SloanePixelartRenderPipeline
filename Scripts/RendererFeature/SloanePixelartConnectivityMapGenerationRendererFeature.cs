using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartConnectivityMapGenerationRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private float m_Threshold = 0.5f;
        [SerializeField]
        private ComputeShader m_ConnectionCheckShader;
        private ComputeShaderPass m_GenerationPass;

        public override void Create()
        {
            m_GenerationPass = new ComputeShaderPass(m_ConnectionCheckShader, "Main", "GenerateConnectivityMap", 8, 8, BeforeDispatch)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            renderer.EnqueuePass(m_GenerationPass);
            m_GenerationPass.ClearSourceBuffers();
            m_GenerationPass.AddSourceBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Depth), pixelartCamera.GetBuffer(TargetBuffer.Depth));
            m_GenerationPass.AddSourceBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Normal), pixelartCamera.GetBuffer(TargetBuffer.Normal));
            m_GenerationPass.SetTargetBuffer(ShaderPropertyStorage.ConnectivityMap, pixelartCamera.GetBuffer(TargetBuffer.Connection));
        }

        private void BeforeDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            cmd.SetComputeFloatParam(computeShader, ShaderPropertyStorage.Threshold, m_Threshold);
            cmd.SetComputeIntParam(computeShader, ShaderPropertyStorage.SamplingScale, pixelartCamera.DownSamplingScale);
        }
    }
}
