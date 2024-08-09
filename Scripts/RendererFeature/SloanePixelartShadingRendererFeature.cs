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
        [SerializeField]
        private float m_AAScaler = 1.75f;
        private BeforeShadingPass m_BeforeShadingPass;
        private ShaderBlitPass m_DiffuseShadingPass;

        public override void Create()
        {
            m_BeforeShadingPass = new BeforeShadingPass(m_AAScaler)
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };

            m_DiffuseShadingPass = new ShaderBlitPass(m_DiffuseShadingShader, "Diffuse Shading")
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_BeforeShadingPass);
            renderer.EnqueuePass(m_DiffuseShadingPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            m_DiffuseShadingPass.SetTargetBuffer(pixelartCamera.GetBuffer(TargetBuffer.Diffuse));
            m_DiffuseShadingPass.SetSourceBuffer(pixelartCamera.GetBuffer(TargetBuffer.Albedo));
        }
    }

    public class BeforeShadingPass : ScriptableRenderPass
    {
        static ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SloaneBeforeShadingPass");
        private float m_AAScaler;
        
        public BeforeShadingPass(float aaScaler)
        {
            m_AAScaler = aaScaler;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            var camera = renderingData.cameraData.camera;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.SetGlobalFloat(ShaderPropertyStorage.ConnectivityAntialiasingThreshold, (float)(pixelartCamera.DownSamplingScale / 2 + m_AAScaler) / pixelartCamera.DownSamplingScale);
                cmd.SetGlobalInt(ShaderPropertyStorage.AdditionalLightCount, renderingData.lightData.additionalLightsCount);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
