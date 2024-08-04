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
        private float m_AAThresholdScale = 1.0f;
        [SerializeField]
        private Shader m_DiffuseShadingShader;
        private BeforeShadingPass m_BeforeShadingPass;
        private ShaderBlitPass m_DiffuseShadingPass;

        public override void Create()
        {
            m_BeforeShadingPass = new BeforeShadingPass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };

            m_DiffuseShadingPass = new ShaderBlitPass("Diffuse Shading", m_DiffuseShadingShader, TargetBuffer.Albedo, TargetBuffer.Diffuse)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_BeforeShadingPass);
            renderer.EnqueuePass(m_DiffuseShadingPass);
        }
    }

    public class BeforeShadingPass : ScriptableRenderPass
    {
        static ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SloaneBeforeShadingPass");
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            var camera = renderingData.cameraData.camera;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.SetGlobalFloat(ShaderPropertyStorage.ConnectivityAntialiasingThreshold, (float)(pixelartCamera.DownSamplingScale / 2 + 1) / pixelartCamera.DownSamplingScale);
                cmd.SetGlobalInt(ShaderPropertyStorage.AdditionalLightCount, renderingData.lightData.additionalLightsCount);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
