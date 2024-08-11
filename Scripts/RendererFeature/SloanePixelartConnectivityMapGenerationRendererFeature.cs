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
        private float m_IterationScale = 1.5f;
        [SerializeField]
        private ComputeShader m_ConnectivityCheckShader;
        [SerializeField]
        private ComputeShader m_ConnectivityFloodShader;
        [SerializeField]
        private ComputeShader m_ConnectivityResultShader;
        private ComputeShaderPass m_GenerationPass;
        private ComputeShaderPass m_FloodingPass;
        private ComputeShaderPass m_ResultingPass;
        protected static readonly int m_FloodingBlitBufferId = Shader.PropertyToID("ConnectivityMapFlooding");
        protected static readonly RenderTargetIdentifier m_FloodingBlitBufferIdentifier = new RenderTargetIdentifier(m_FloodingBlitBufferId);

        public override void Create()
        {
            if (m_ConnectivityCheckShader == null || m_ConnectivityFloodShader == null || m_ConnectivityResultShader == null) return;

            m_GenerationPass = new ComputeShaderPass(m_ConnectivityCheckShader, "Main", "ConnectivityMapGeneration", 8, 8, BeforeGenerateDispatch)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

            m_FloodingPass = new ComputeShaderPass(m_ConnectivityFloodShader, "Main", "ConnectivityMapFlooding", 8, 8, BeforeFloodDispatch, AfterFloodDispatch)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

            m_ResultingPass = new ComputeShaderPass(m_ConnectivityResultShader, "Main", "ConnectivityMapResulting", 8, 8, BeforeResultDispatch, AfterResultDispatch)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_GenerationPass == null || m_FloodingPass == null || m_ResultingPass == null) return;

            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            var connectivityMap = pixelartCamera.GetBuffer(TargetBuffer.ConnectivityDetail);
            var depthBuffer = pixelartCamera.GetBuffer(TargetBuffer.Depth);
            var normalBuffer = pixelartCamera.GetBuffer(TargetBuffer.Normal);

            m_GenerationPass.ClearSourceBuffers();
            m_GenerationPass.AddSourceBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Depth), depthBuffer);
            m_GenerationPass.AddSourceBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Normal), normalBuffer);
            m_GenerationPass.SetTargetBuffer(ShaderPropertyStorage.ConnectivityMap, connectivityMap);
            renderer.EnqueuePass(m_GenerationPass);

            m_FloodingPass.ClearSourceBuffers();
            m_FloodingPass.AddSourceBuffer(ShaderPropertyStorage.PrevConnectivityMap, m_FloodingBlitBufferIdentifier, connectivityMap.width, connectivityMap.height);
            m_FloodingPass.SetTargetBuffer(ShaderPropertyStorage.ConnectivityMap, connectivityMap);

            int interationTime = Mathf.RoundToInt(pixelartCamera.DownSamplingScale * m_IterationScale);
            for (int i = 0; i < interationTime; i++) renderer.EnqueuePass(m_FloodingPass);

            m_ResultingPass.ClearSourceBuffers();
            m_ResultingPass.AddSourceBuffer(ShaderPropertyStorage.ConnectivityMap, connectivityMap);
            m_GenerationPass.AddSourceBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Depth), depthBuffer);
            m_GenerationPass.AddSourceBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Normal), normalBuffer);
            m_ResultingPass.SetTargetBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.ConnectivityResult), pixelartCamera.GetBuffer(TargetBuffer.ConnectivityResult));
            renderer.EnqueuePass(m_ResultingPass);
        }

        private void BeforeGenerateDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            cmd.SetComputeFloatParam(computeShader, ShaderPropertyStorage.Threshold, m_Threshold);
            cmd.SetComputeIntParam(computeShader, ShaderPropertyStorage.SamplingScale, pixelartCamera.DownSamplingScale);
        }

        private void BeforeFloodDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            var ConnectivityBuffer = pixelartCamera.GetBuffer(TargetBuffer.ConnectivityDetail);
            RenderTextureDescriptor tempDes = ConnectivityBuffer.descriptor;
            cmd.GetTemporaryRT(m_FloodingBlitBufferId, tempDes);
            cmd.Blit(ConnectivityBuffer, m_FloodingBlitBufferIdentifier);

            cmd.SetComputeIntParam(computeShader, ShaderPropertyStorage.SamplingScale, pixelartCamera.DownSamplingScale);
        }

        private void AfterFloodDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            cmd.ReleaseTemporaryRT(m_FloodingBlitBufferId);
        }

        private void BeforeResultDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            cmd.SetComputeFloatParam(computeShader, ShaderPropertyStorage.Threshold, m_Threshold);
            cmd.SetComputeIntParam(computeShader, ShaderPropertyStorage.SamplingScale, pixelartCamera.DownSamplingScale);
        }

        private void AfterResultDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            for (int i = (int)TargetBufferStage.MarkerConnectivityDetail; i <= (int)TargetBufferStage.MarkerConnectivityResult; i++)
            {
                cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty((TargetBuffer)i), pixelartCamera.GetBuffer((TargetBuffer)i));
            }
        }
    }
}
