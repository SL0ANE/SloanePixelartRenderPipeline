
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class ComputeShaderPass : ScriptableRenderPass
    {
        // Well actually
        private struct ComputeShaderBuffer
        {
            public int BufferName;
            public int Width;
            public int Height;
            public RenderTargetIdentifier BufferIdentifier;
        }

        static ProfilingSampler m_ProfilingSampler;
        ComputeShader m_ComputeShader;
        int m_ComputeShaderKernelIndex;
        ComputeShaderBuffer m_TargetBuffer;
        List<ComputeShaderBuffer> m_SourceBuffers;
        Action<CommandBuffer, RenderingData, ComputeShader> m_CallbackBeforeDispatch;
        Action<CommandBuffer, RenderingData, ComputeShader> m_CallbackAfterDispatch;
        Vector2Int m_ThreadNum;
        public ComputeShaderPass(ComputeShader computeShader, string kernelName, string profilingName, int numX = 8, int numY = 8, Action<CommandBuffer, RenderingData, ComputeShader> callbackBeforeDispatch = null, Action<CommandBuffer, RenderingData, ComputeShader> callbackAfterDispatch = null)
        {
            m_ComputeShader = computeShader;
            m_ComputeShaderKernelIndex = m_ComputeShader.FindKernel(kernelName);
            m_ProfilingSampler = new ProfilingSampler(profilingName);
            m_SourceBuffers = new List<ComputeShaderBuffer>();

            m_CallbackBeforeDispatch = callbackBeforeDispatch;
            m_CallbackAfterDispatch = callbackAfterDispatch;

            m_ThreadNum = new Vector2Int(numX, numY);
        }

        public void SetTargetBuffer(string name, RenderTexture buffer)
        {
            SetTargetBuffer(name, buffer, buffer.width, buffer.height);
        }

        public void SetTargetBuffer(string name, RenderTargetIdentifier identifier, int width, int height)
        {
            m_TargetBuffer = new ComputeShaderBuffer()
            {
                BufferName = Shader.PropertyToID(name),
                Width = width,
                Height = height,
                BufferIdentifier = identifier
            };
        }

        public void AddSourceBuffer(string name, RenderTexture buffer)
        {
            AddSourceBuffer(name, buffer, buffer.width, buffer.height);
        }

        public void ClearSourceBuffers()
        {
            m_SourceBuffers.Clear();
        }

        public void AddSourceBuffer(string name, RenderTargetIdentifier identifier, int width, int height)
        {
            var sourceBuffer = new ComputeShaderBuffer()
            {
                BufferName = Shader.PropertyToID(name),
                Width = width,
                Height = height,
                BufferIdentifier = identifier
            };

            m_SourceBuffers.Add(sourceBuffer);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                foreach (var buffer in m_SourceBuffers)
                {
                    cmd.SetComputeTextureParam(m_ComputeShader, m_ComputeShaderKernelIndex, buffer.BufferName, buffer.BufferIdentifier);
                }

                cmd.SetComputeTextureParam(m_ComputeShader, m_ComputeShaderKernelIndex, m_TargetBuffer.BufferName, m_TargetBuffer.BufferIdentifier);
                cmd.SetComputeIntParam(m_ComputeShader, ShaderPropertyStorage.Width, m_TargetBuffer.Width);
                cmd.SetComputeIntParam(m_ComputeShader, ShaderPropertyStorage.Height, m_TargetBuffer.Height);

                m_CallbackBeforeDispatch?.Invoke(cmd, renderingData, m_ComputeShader);

                // cmd.SetRenderTarget(m_TargetBuffer.BufferIdentifier);
                cmd.DispatchCompute(m_ComputeShader, m_ComputeShaderKernelIndex, m_TargetBuffer.Width / m_ThreadNum.x, m_TargetBuffer.Height / m_ThreadNum.y, 1);

                m_CallbackAfterDispatch?.Invoke(cmd, renderingData, m_ComputeShader);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}