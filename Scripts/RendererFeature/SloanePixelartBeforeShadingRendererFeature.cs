using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartBeforeShadingRendererFeature : ScriptableRendererFeature
    {
        private SloanePixelartBeforeShadingRendererFeaturePass m_BeforeShadingPass;
        public override void Create()
        {
            m_BeforeShadingPass = new SloanePixelartBeforeShadingRendererFeaturePass()
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_BeforeShadingPass);
        }
    }

    public class SloanePixelartBeforeShadingRendererFeaturePass : ScriptableRenderPass
    {
        static ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SloaneBeforeShadingPass");
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelArtCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                for (int i = (int)TargetBufferStage.Start + 1; i <= (int)TargetBufferStage.StageRenderObjects; i++)
                {
                    cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty((TargetBuffer)i), pixelArtCamera.GetBuffer((TargetBuffer)i));
                }

                cmd.SetGlobalTexture(TargetBufferUtil.DepthBufferShaderProperty, pixelArtCamera.DepthBuffer);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
