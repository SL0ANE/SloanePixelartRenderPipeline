using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartDownSamplingRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private ComputeShader m_DownSamplingComputeShader;
        private SloanePixelartDownSamplingRendererFeaturePass m_DownSamplingPass;

        public override void Create()
        {
            m_DownSamplingPass = new SloanePixelartDownSamplingRendererFeaturePass(m_DownSamplingComputeShader)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_DownSamplingPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                m_DownSamplingPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
            }
        }
    }

    public class SloanePixelartDownSamplingRendererFeaturePass : CameraTargetRendererFeaturePass
    {
        static ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SloanePixelartDownSampling");
        static int m_RenderTextureName = Shader.PropertyToID("DownSamplingTarget");
        ComputeShader m_DownSamplingComputerShader;
        int m_DownSamplingComputerShaderKernelIndex;
        public SloanePixelartDownSamplingRendererFeaturePass(ComputeShader computeShader)
        {
            m_DownSamplingComputerShader = computeShader;
            m_DownSamplingComputerShaderKernelIndex = m_DownSamplingComputerShader.FindKernel("DownSamplingBase");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelArtCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.GetTemporaryRT(m_RenderTextureName, pixelArtCamera.TargetWidth, pixelArtCamera.TargetHeight, m_CameraColorTarget.rt.depth, FilterMode.Point, m_CameraColorTarget.rt.graphicsFormat, 1, true);
                
                cmd.SetComputeTextureParam(m_DownSamplingComputerShader, m_DownSamplingComputerShaderKernelIndex, ShaderPropertyStorage.Source, m_CameraColorTarget);
                cmd.SetComputeTextureParam(m_DownSamplingComputerShader, m_DownSamplingComputerShaderKernelIndex, ShaderPropertyStorage.Target, m_RenderTextureName);
                cmd.SetComputeIntParam(m_DownSamplingComputerShader, ShaderPropertyStorage.Width, pixelArtCamera.TargetWidth);
                cmd.SetComputeIntParam(m_DownSamplingComputerShader, ShaderPropertyStorage.Height, pixelArtCamera.TargetHeight);
                cmd.SetComputeIntParam(m_DownSamplingComputerShader, ShaderPropertyStorage.SamplingScale, pixelArtCamera.DownSamplingScale);

                cmd.DispatchCompute(m_DownSamplingComputerShader, m_DownSamplingComputerShaderKernelIndex, pixelArtCamera.TargetWidth / 8, pixelArtCamera.TargetHeight / 8, 1);
                cmd.Blit(m_RenderTextureName, m_CameraColorTarget);
                cmd.ReleaseTemporaryRT(m_RenderTextureName);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
