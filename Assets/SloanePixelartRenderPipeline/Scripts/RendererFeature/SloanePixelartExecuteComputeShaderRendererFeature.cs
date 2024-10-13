using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartExecuteComputeShaderRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private ComputeShader m_Shader;
        [SerializeField]
        private string m_KernelName = "Main";
        [SerializeField]
        private List<TargetBuffer> m_ExtraSourceBuffers = new List<TargetBuffer>();
        [SerializeField]
        private TargetBuffer m_TargetBuffer;
        private ComputeShaderPass m_ExecutePass;
        protected static readonly int m_BlitBufferId = Shader.PropertyToID("ExecuteComputeShaderPassBlit");
        protected static readonly RenderTargetIdentifier m_BlitBufferIdentifier = new RenderTargetIdentifier(m_BlitBufferId);

        public override void Create()
        {
            if (m_Shader == null) return;

            m_ExecutePass = new ComputeShaderPass(m_Shader, m_KernelName, "Execute Compute Shader Pass", 8, 8, BeforeDispatch, AfterDispatch)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_Shader == null) return;

            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);

            m_ExecutePass.ClearSourceBuffers();
            var targetBuffer = pixelartCamera.GetBuffer(m_TargetBuffer);

            foreach (var sourceBuffer in m_ExtraSourceBuffers)
            {
                m_ExecutePass.AddSourceBuffer(TargetBufferUtil.GetBufferShaderProperty(sourceBuffer), pixelartCamera.GetBuffer(sourceBuffer));
            }

            m_ExecutePass.AddSourceBuffer(ShaderPropertyStorage.Source, m_BlitBufferIdentifier, targetBuffer.width, targetBuffer.height);
            m_ExecutePass.SetTargetBuffer(ShaderPropertyStorage.Target, pixelartCamera.GetBuffer(m_TargetBuffer));

            renderer.EnqueuePass(m_ExecutePass);
        }

        private void BeforeDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            var targetBuffer = pixelartCamera.GetBuffer(m_TargetBuffer);

            cmd.GetTemporaryRT(m_BlitBufferId, targetBuffer.descriptor);
            cmd.SetRenderTarget(m_BlitBufferIdentifier);
            cmd.Blit(targetBuffer, m_BlitBufferIdentifier);
            cmd.SetComputeIntParam(computeShader, ShaderPropertyStorage.SamplingScale, pixelartCamera.DownSamplingScale);
        }

        private void AfterDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            cmd.ReleaseTemporaryRT(m_BlitBufferId);
        }
    }
}
