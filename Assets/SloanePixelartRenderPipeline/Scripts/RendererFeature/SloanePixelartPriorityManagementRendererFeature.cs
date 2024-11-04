using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartPriorityManagementRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private ComputeShader m_PriorityManagementShader;
        [SerializeField, Min(0.0f)]
        private float m_CenterDistanceAttenuation = 1.0f;
        private ComputeShaderPass m_PriorityManagementPass;

        public override void Create()
        {
            if (m_PriorityManagementShader == null) return;

            m_PriorityManagementPass = new ComputeShaderPass(m_PriorityManagementShader, "Main", "PriorityManagement", 8, 8, BeforeDispatch, AfterDispatch)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_PriorityManagementShader == null) return;

            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);

            m_PriorityManagementPass.ClearSourceBuffers();
            m_PriorityManagementPass.AddSourceBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.ShapeProperty), pixelartCamera.GetBuffer(TargetBuffer.ShapeProperty));
            m_PriorityManagementPass.AddSourceBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Albedo), pixelartCamera.GetBuffer(TargetBuffer.Albedo));
            m_PriorityManagementPass.SetTargetBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.UV), pixelartCamera.GetBuffer(TargetBuffer.UV));
            m_PriorityManagementPass.SetTargetBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.ConnectivityDetail), pixelartCamera.GetBuffer(TargetBuffer.ConnectivityDetail));
            renderer.EnqueuePass(m_PriorityManagementPass);
        }

        private void BeforeDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            cmd.SetComputeIntParam(computeShader, ShaderPropertyStorage.SamplingScale, pixelartCamera.DownSamplingScale);
            cmd.SetComputeFloatParam(computeShader, ShaderPropertyStorage.CenterDistanceAttenuation, m_CenterDistanceAttenuation);
        }

        private void AfterDispatch(CommandBuffer cmd, RenderingData renderingData, ComputeShader computeShader)
        {
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);
            cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.UV), pixelartCamera.GetBuffer(TargetBuffer.UV));
        }
    }
}
