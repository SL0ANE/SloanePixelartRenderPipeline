
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    // Well actually
    public struct ShaderInputBuffer
    {
        public int BufferName;
        public RenderTargetIdentifier BufferIdentifier;
    }

    public class ComputeShaderPass : ScriptableRenderPass
    {

        ProfilingSampler m_ProfilingSampler;
        ComputeShader m_ComputeShader;
        int m_ComputeShaderKernelIndex;
        ShaderInputBuffer m_TargetBuffer;
        int m_TargetWidth;
        int m_TargetHeight;
        List<ShaderInputBuffer> m_SourceBuffers;
        Action<CommandBuffer, RenderingData, ComputeShader> m_CallbackBeforeDispatch;
        Action<CommandBuffer, RenderingData, ComputeShader> m_CallbackAfterDispatch;
        Vector2Int m_ThreadNum;
        public ComputeShaderPass(ComputeShader computeShader, string kernelName, string profilingName, int numX = 8, int numY = 8, Action<CommandBuffer, RenderingData, ComputeShader> callbackBeforeDispatch = null, Action<CommandBuffer, RenderingData, ComputeShader> callbackAfterDispatch = null)
        {
            m_ComputeShader = computeShader;
            m_ComputeShaderKernelIndex = m_ComputeShader.FindKernel(kernelName);
            m_ProfilingSampler = new ProfilingSampler(profilingName);
            m_SourceBuffers = new List<ShaderInputBuffer>();

            m_CallbackBeforeDispatch = callbackBeforeDispatch;
            m_CallbackAfterDispatch = callbackAfterDispatch;

            m_ThreadNum = new Vector2Int(numX, numY);
        }

        public void SetTargetBuffer(int name, RenderTexture buffer)
        {
            SetTargetBuffer(name, buffer, buffer.width, buffer.height);
        }

        public void SetTargetBuffer(string name, RenderTexture buffer)
        {
            SetTargetBuffer(Shader.PropertyToID(name), buffer, buffer.width, buffer.height);
        }

        public void SetTargetBuffer(int name, RenderTargetIdentifier identifier, int width, int height)
        {
            m_TargetBuffer = new ShaderInputBuffer()
            {
                BufferName = name,
                BufferIdentifier = identifier
            };

            m_TargetWidth = width;
            m_TargetHeight = height;
        }

        public void AddSourceBuffer(int name, RenderTexture buffer)
        {
            AddSourceBuffer(name, buffer, buffer.width, buffer.height);
        }

        public void AddSourceBuffer(string name, RenderTexture buffer)
        {
            AddSourceBuffer(Shader.PropertyToID(name), buffer, buffer.width, buffer.height);
        }

        public void ClearSourceBuffers()
        {
            m_SourceBuffers.Clear();
        }

        public void AddSourceBuffer(int name, RenderTargetIdentifier identifier, int width, int height)
        {
            var sourceBuffer = new ShaderInputBuffer()
            {
                BufferName = name,
                BufferIdentifier = identifier
            };

            m_SourceBuffers.Add(sourceBuffer);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                m_CallbackBeforeDispatch?.Invoke(cmd, renderingData, m_ComputeShader);

                foreach (var buffer in m_SourceBuffers)
                {
                    cmd.SetComputeTextureParam(m_ComputeShader, m_ComputeShaderKernelIndex, buffer.BufferName, buffer.BufferIdentifier);
                }

                cmd.SetComputeTextureParam(m_ComputeShader, m_ComputeShaderKernelIndex, m_TargetBuffer.BufferName, m_TargetBuffer.BufferIdentifier);
                cmd.SetComputeIntParam(m_ComputeShader, ShaderPropertyStorage.Width, m_TargetWidth);
                cmd.SetComputeIntParam(m_ComputeShader, ShaderPropertyStorage.Height, m_TargetHeight);
                cmd.SetRenderTarget(m_TargetBuffer.BufferIdentifier);

                // cmd.SetRenderTarget(m_TargetBuffer.BufferIdentifier);
                cmd.DispatchCompute(m_ComputeShader, m_ComputeShaderKernelIndex, Mathf.CeilToInt((float)m_TargetWidth / m_ThreadNum.x), Mathf.CeilToInt((float)m_TargetHeight / m_ThreadNum.y), 1);

                m_CallbackAfterDispatch?.Invoke(cmd, renderingData, m_ComputeShader);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}