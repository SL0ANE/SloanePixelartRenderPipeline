using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartCopyTargetRendererFeature : ScriptableRendererFeature
    {
        private SloanePixelartCopyTargetRendererFeaturePass m_CopyTargetPass;
        public override void Create()
        {
            m_CopyTargetPass = new SloanePixelartCopyTargetRendererFeaturePass
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_CopyTargetPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                m_CopyTargetPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
            }
        }
    }

    public class SloanePixelartCopyTargetRendererFeaturePass : CameraTargetRendererFeaturePass
    {
        static ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SloanePixelartCopyTarget");
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelArtCamera = SloanePixelartCamera.GetPixelartCamera(camera);

            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.SetRenderTarget(m_CameraColorTarget);
                cmd.Blit(pixelArtCamera.ResultBuffer, m_CameraColorTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
