using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public class SloanePixelartShadingRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Shader m_ShadingShader;
        [SerializeField]
        private ComputeShader m_RimLightComputeShader;
        [SerializeField]
        private bool m_EnableAA = true;
        [SerializeField]
        private float m_AAScaler = 1.75f;
        private BeforeShadingPass m_BeforeShadingPass;
        private ShaderBlitPass m_DiffuseShadingPass;
        private ShaderBlitPass m_SpecularShadingPass;
        private ShaderBlitPass m_GlobalIlluminationShadingPass;
        private ShaderBlitPass m_RimLightShadingPass;
        private ComputeShaderPass m_RimLightComputePass;
        private ShaderBlitPass m_CombinationPass;
        private ComputeBuffer m_RimLightBuffer;

        public override void Create()
        {
            if (m_ShadingShader == null || m_RimLightComputeShader == null) return;

            m_BeforeShadingPass = new BeforeShadingPass(m_AAScaler * (m_EnableAA ? 1.0f : 0.0f))
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };

            m_DiffuseShadingPass = new ShaderBlitPass(m_ShadingShader, "Diffuse Shading", null, null, 0)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

            m_SpecularShadingPass = new ShaderBlitPass(m_ShadingShader, "Specular Shading", null, null, 1)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

            m_GlobalIlluminationShadingPass = new ShaderBlitPass(m_ShadingShader, "Global Illumination Shading", null, null, 2)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

            m_RimLightShadingPass = new ShaderBlitPass(m_ShadingShader, "Rim Light Shading", BeforeRimLightShading, null, 3)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

            m_RimLightComputePass = new ComputeShaderPass(m_RimLightComputeShader, "Main", "ConnectivityMapGeneration", 8, 8)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

            m_CombinationPass = new ShaderBlitPass(m_ShadingShader, "Shading Combination", null, null, 4)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };

            if (m_RimLightBuffer == null)
            {
                m_RimLightBuffer = new ComputeBuffer(SloanePixelartRimlight.MAX_RIMLIGHTS, sizeof(float) * 6);
            }
        }

        private void BeforeRimLightShading(CommandBuffer cmd, RenderingData renderingData)
        {
            SloanePixelartRimlight.RefreshRimLights();
            m_RimLightBuffer.SetData(SloanePixelartRimlight.RimLights);
            cmd.SetGlobalBuffer(ShaderPropertyStorage.RimLights, m_RimLightBuffer);
            cmd.SetGlobalInt(ShaderPropertyStorage.RimLightsCount, SloanePixelartRimlight.RimLightsCount);
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_ShadingShader == null || m_RimLightComputeShader == null) return;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(renderingData.cameraData.camera, SloanePixelartCamera.CameraTarget.CastCamera);

            renderer.EnqueuePass(m_BeforeShadingPass);

            renderer.EnqueuePass(m_RimLightShadingPass);
            m_RimLightComputePass.ClearSourceBuffers();
            m_RimLightComputePass.SetTargetBuffer(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.RimLight), pixelartCamera.GetBuffer(TargetBuffer.RimLight));
            renderer.EnqueuePass(m_RimLightComputePass);


            renderer.EnqueuePass(m_DiffuseShadingPass);
            renderer.EnqueuePass(m_SpecularShadingPass);
            renderer.EnqueuePass(m_GlobalIlluminationShadingPass);
            renderer.EnqueuePass(m_CombinationPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (m_ShadingShader == null || m_RimLightComputeShader == null) return;

            var camera = renderingData.cameraData.camera;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);
            var albedoBuffer = pixelartCamera.GetBuffer(TargetBuffer.Diffuse);

            m_DiffuseShadingPass.SetSourceBuffer(albedoBuffer);
            m_DiffuseShadingPass.SetTargetBuffer(pixelartCamera.GetBuffer(TargetBuffer.Diffuse));

            m_SpecularShadingPass.SetSourceBuffer(albedoBuffer);
            m_SpecularShadingPass.SetTargetBuffer(pixelartCamera.GetBuffer(TargetBuffer.Specular));

            m_GlobalIlluminationShadingPass.SetSourceBuffer(albedoBuffer);
            m_GlobalIlluminationShadingPass.SetTargetBuffer(pixelartCamera.GetBuffer(TargetBuffer.GlobalIllumination));

            m_RimLightShadingPass.SetSourceBuffer(albedoBuffer);
            m_RimLightShadingPass.SetTargetBuffer(pixelartCamera.GetBuffer(TargetBuffer.RimLight));

            m_CombinationPass.SetSourceBuffer(albedoBuffer);
            m_CombinationPass.SetTargetBuffer(renderer.cameraColorTargetHandle);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && m_RimLightBuffer != null)
            {
                // Release the ComputeBuffer to free up GPU memory
                m_RimLightBuffer.Release();
                m_RimLightBuffer = null;
            }

            base.Dispose(disposing);
        }
    }

    public class BeforeShadingPass : ScriptableRenderPass
    {
        static ProfilingSampler m_ProfilingSampler = new ProfilingSampler("SloaneBeforeShadingPass");
        private float m_AAScaler;

        public BeforeShadingPass(float aaScaler)
        {
            m_AAScaler = aaScaler;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();
            var camera = renderingData.cameraData.camera;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                var diffuseBuffer = pixelartCamera.GetBuffer(TargetBuffer.Diffuse);
                var specularBuffer = pixelartCamera.GetBuffer(TargetBuffer.Specular);
                var globalIlluminationBuffer = pixelartCamera.GetBuffer(TargetBuffer.GlobalIllumination);
                var rimLightBuffer = pixelartCamera.GetBuffer(TargetBuffer.RimLight);

                cmd.SetRenderTarget(diffuseBuffer);
                cmd.ClearRenderTarget(true, true, Color.clear, 1);
                cmd.SetRenderTarget(specularBuffer);
                cmd.ClearRenderTarget(true, true, Color.clear, 1);
                cmd.SetRenderTarget(globalIlluminationBuffer);
                cmd.ClearRenderTarget(true, true, Color.clear, 1);
                cmd.SetRenderTarget(rimLightBuffer);
                cmd.ClearRenderTarget(true, true, Color.clear, 1);

                cmd.SetGlobalVector(ShaderPropertyStorage.ScreenParams, new Vector4(pixelartCamera.TargetWidth, pixelartCamera.TargetHeight, 1.0f + 1.0f / pixelartCamera.TargetWidth, 1.0f + 1.0f / pixelartCamera.TargetHeight));
                cmd.SetGlobalFloat(ShaderPropertyStorage.ConnectivityAntialiasingThreshold, m_AAScaler / 2.0f);
                cmd.SetGlobalInt(ShaderPropertyStorage.AdditionalLightCount, renderingData.lightData.additionalLightsCount);

                cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Diffuse), diffuseBuffer);
                cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Specular), specularBuffer);
                cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.GlobalIllumination), globalIlluminationBuffer);
                cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.RimLight), rimLightBuffer);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
