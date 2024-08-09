using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartBeforeRenderRendererFeature : ScriptableRendererFeature
    {
        private SloanePixelartBeforeRenderRendererFeaturePass m_BeforeRenderPass;

        public override void Create()
        {
            m_BeforeRenderPass = new SloanePixelartBeforeRenderRendererFeaturePass()
            {
                renderPassEvent = RenderPassEvent.BeforeRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_BeforeRenderPass);
        }
    }

    public class SloanePixelartBeforeRenderRendererFeaturePass : ScriptableRenderPass
    {
        static ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SloanePixelartDebug");

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            var cmd = CommandBufferPool.Get();
            float unitSize = pixelartCamera.UnitSize;

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                // Align the camera to the unit size so the shape won't dither when the camera moves.

                Matrix4x4 viewMatrix = renderingData.cameraData.GetViewMatrix();
                var proj = renderingData.cameraData.GetProjectionMatrix();
                cmd.SetGlobalMatrix(ShaderPropertyStorage.CameraViewMatrix, viewMatrix);
                cmd.SetGlobalMatrix(ShaderPropertyStorage.CameraInvViewMatrix, viewMatrix.inverse);
                var viewProjMat = proj * viewMatrix;
                cmd.SetGlobalMatrix(ShaderPropertyStorage.CameraViewProjectionMatrix, viewProjMat);
                cmd.SetGlobalMatrix(ShaderPropertyStorage.CameraInvViewProjectionMatrix, viewProjMat.inverse);

                cmd.SetProjectionMatrix(proj);
                cmd.SetGlobalFloat(ShaderPropertyStorage.UnitSize, unitSize);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
