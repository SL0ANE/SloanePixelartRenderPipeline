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
            var pixelArtCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            var cmd = CommandBufferPool.Get();

            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(TargetShader, ref renderingData, sortingCriteria);
            float unitSize = pixelArtCamera.UnitSize;

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                // Align the camera to the unit size so the shape won't dither when the camera moves.
                
                Matrix4x4 viewMatrix = renderingData.cameraData.GetViewMatrix();
                Vector4 cameraTranslation = viewMatrix.GetColumn(3);
                float alignedX = Mathf.Round(cameraTranslation.x / unitSize) * unitSize;
                float alignedY = Mathf.Round(cameraTranslation.y / unitSize) * unitSize;
                cameraTranslation = new Vector4(alignedX, alignedY, cameraTranslation.z, cameraTranslation.w);
                viewMatrix.SetColumn(3, cameraTranslation);
                cmd.SetGlobalMatrix(ShaderPropertyStorage.ViewMatrix, viewMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyStorage.InvViewMatrix, viewMatrix.inverse);
                var proj = renderingData.cameraData.GetProjectionMatrix();
                proj.SetColumn(1, -1 * proj.GetColumn(1));
                cmd.SetProjectionMatrix(proj);
                cmd.SetInvertCulling(true);
                cmd.SetGlobalFloat(ShaderPropertyStorage.UnitSize, unitSize);

                cmd.SetRenderTarget(pixelArtCamera.MultiBufferIdentifiers, pixelArtCamera.DepthBuffer);
                cmd.ClearRenderTarget(true, true, Color.black, 1);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings);

                for (int i = (int)TargetBufferStage.Start + 1; i <= (int)TargetBufferStage.StageRenderObjects; i++)
                {
                    cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty((TargetBuffer)i), pixelArtCamera.GetBuffer((TargetBuffer)i));
                }

                cmd.SetGlobalTexture(TargetBufferUtil.DepthBufferShaderProperty, pixelArtCamera.DepthBuffer);
                cmd.SetInvertCulling(false);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
