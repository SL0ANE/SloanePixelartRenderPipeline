using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class ShaderBlitPass : ScriptableRenderPass
    {

        static ProfilingSampler m_ProfilingSampler;
        Material m_Material;
        RenderTexture m_TargetBuffer;
        RenderTexture m_SourceBuffer;
        Action<CommandBuffer, RenderingData> m_CallbackBeforeBlit;
        Action<CommandBuffer, RenderingData> m_CallbackAfterBlit;

        protected static readonly int m_DuplicateCaseBlitBufferId = Shader.PropertyToID("DuplicateCaseBlit");

        public ShaderBlitPass(Shader shader, string profilingName, Action<CommandBuffer, RenderingData> callbackBeforeBlit = null, Action<CommandBuffer, RenderingData> callbackAfterBlit = null)
        {
            m_Material = new Material(shader);
            m_ProfilingSampler = new ProfilingSampler(profilingName);

            m_CallbackBeforeBlit = callbackBeforeBlit;
            m_CallbackAfterBlit = callbackAfterBlit;
        }

        public void SetTargetBuffer(RenderTexture identifier)
        {
            m_TargetBuffer = identifier;
        }

        public void SetSourceBuffer(RenderTexture identifier)
        {
            m_SourceBuffer = identifier;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                m_CallbackBeforeBlit?.Invoke(cmd, renderingData);

                if (m_TargetBuffer == m_SourceBuffer)
                {
                    cmd.GetTemporaryRT(m_DuplicateCaseBlitBufferId, m_TargetBuffer.descriptor);

                    cmd.SetRenderTarget(m_DuplicateCaseBlitBufferId);
                    cmd.Blit(m_SourceBuffer, m_DuplicateCaseBlitBufferId);

                    cmd.SetRenderTarget(m_TargetBuffer);
                    cmd.SetGlobalTexture(ShaderPropertyStorage.MainTex, m_DuplicateCaseBlitBufferId);
                    cmd.Blit(m_DuplicateCaseBlitBufferId, m_TargetBuffer, m_Material);

                    cmd.ReleaseTemporaryRT(m_DuplicateCaseBlitBufferId);
                }
                else
                {
                    cmd.SetRenderTarget(m_TargetBuffer);
                    cmd.Blit(m_SourceBuffer, m_TargetBuffer, m_Material);
                }

                m_CallbackAfterBlit?.Invoke(cmd, renderingData);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
