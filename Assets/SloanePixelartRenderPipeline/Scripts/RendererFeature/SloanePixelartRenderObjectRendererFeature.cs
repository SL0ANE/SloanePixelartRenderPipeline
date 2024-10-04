using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartOpaqueRenderRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private LayerMask m_LayerMask;
        private SloanePixelartOpaqueRenderRendererFeaturePass m_RenderObjectPass;
        public override void Create()
        {
            m_RenderObjectPass = new SloanePixelartOpaqueRenderRendererFeaturePass(m_LayerMask)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_RenderObjectPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {

        }
    }

    public class SloanePixelartOpaqueRenderRendererFeaturePass : ScriptableRenderPass
    {
        static ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SloanePixelartOpaqueRender");
        public static readonly ShaderTagId TargetShader = new ShaderTagId("PixelartOpaque");
        private FilteringSettings m_FilteringSettings;
        public SloanePixelartOpaqueRenderRendererFeaturePass(LayerMask layerMask)
        {
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cmd.EnableKeyword(SloanePixelartKeywordsStorage.PixelartRendering);
            base.OnCameraSetup(cmd, ref renderingData);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.DisableKeyword(SloanePixelartKeywordsStorage.PixelartRendering);
            base.OnCameraCleanup(cmd);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            var cmd = CommandBufferPool.Get();

            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(TargetShader, ref renderingData, sortingCriteria);

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.SetRenderTarget(pixelartCamera.ResultBuffer);
                cmd.ClearRenderTarget(false, true, camera.backgroundColor, 1);
                cmd.SetRenderTarget(pixelartCamera.OpaqueBuffersIdentifiers, pixelartCamera.GetBuffer(TargetBuffer.Depth));
                cmd.ClearRenderTarget(true, true, Color.clear, 1);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);

                for (int i = (int)TargetBufferStage.Start + 1; i <= (int)TargetBufferStage.StageRenderObjects; i++)
                {
                    cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty((TargetBuffer)i), pixelartCamera.GetBuffer((TargetBuffer)i));
                }
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
