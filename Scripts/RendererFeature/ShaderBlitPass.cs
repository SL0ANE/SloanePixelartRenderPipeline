using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class ShaderBlitPass : ScriptableRenderPass
    {
        private ProfilingSampler m_ProfilingSampler;
        private Material m_Material;
        private TargetBuffer m_SourceBuffer;
        private TargetBuffer m_TargetBuffer;
        public ShaderBlitPass(string profilingName, Shader shader, TargetBuffer sourceBuffer, TargetBuffer targetBuffer)
        {
            m_Material = new Material(shader);
            m_ProfilingSampler = new ProfilingSampler(profilingName);

            m_SourceBuffer = sourceBuffer;
            m_TargetBuffer = targetBuffer;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var pixelArtCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);
            var sourceBuffer = pixelArtCamera.GetBuffer(m_SourceBuffer);
            var targerBuffer = pixelArtCamera.GetBuffer(m_TargetBuffer);

            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.SetRenderTarget(targerBuffer);
                cmd.Blit(sourceBuffer, targerBuffer, m_Material);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
