using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartDebugRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private TargetBuffer m_TargerBuffer;
        [SerializeField, HideInInspector]
        private ComputeShader m_DownSamplingComputeShader;
        private SloanePixelartDebugRendererFeaturePass m_DebugPass;

        public override void Create()
        {
            GlobalKeyword.Create("PIXELART_RENDERING");
            m_DebugPass = new SloanePixelartDebugRendererFeaturePass(m_TargerBuffer)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_DebugPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                m_DebugPass.SetTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
            }
        }
    }

    public class SloanePixelartDebugRendererFeaturePass : CameraTargetRendererFeaturePass
    {
        static ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SloanePixelartDebug");
        private TargetBuffer m_TargerBuffer;
        public SloanePixelartDebugRendererFeaturePass(TargetBuffer targerBuffer)
        {
            m_TargerBuffer = targerBuffer;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelArtCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.SetRenderTarget(m_CameraColorTarget);
                cmd.Blit(pixelArtCamera.GetBuffer(m_TargerBuffer), m_CameraColorTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
