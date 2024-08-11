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
        private float m_AAScaler = 1.75f;
        private BeforeShadingPass m_BeforeShadingPass;
        private ShaderBlitPass m_DiffuseShadingPass;
        private ShaderBlitPass m_SpecularShadingPass;
        private ShaderBlitPass m_CombinationPass;

        public override void Create()
        {
            if(m_ShadingShader == null) return;

            m_BeforeShadingPass = new BeforeShadingPass(m_AAScaler)
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

            m_CombinationPass = new ShaderBlitPass(m_ShadingShader, "Shading Combination", null, null, 3)
            {
                renderPassEvent = RenderPassEvent.AfterRendering
            };
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if(m_ShadingShader == null) return;
            
            renderer.EnqueuePass(m_BeforeShadingPass);
            renderer.EnqueuePass(m_DiffuseShadingPass);
            renderer.EnqueuePass(m_SpecularShadingPass);
            renderer.EnqueuePass(m_CombinationPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if(m_ShadingShader == null) return;

            var camera = renderingData.cameraData.camera;
            var pixelartCamera = SloanePixelartCamera.GetPixelartCamera(camera, SloanePixelartCamera.CameraTarget.CastCamera);
            var albedoBuffer = pixelartCamera.GetBuffer(TargetBuffer.Diffuse);

            m_DiffuseShadingPass.SetSourceBuffer(albedoBuffer);
            m_DiffuseShadingPass.SetTargetBuffer(pixelartCamera.GetBuffer(TargetBuffer.Diffuse));

            m_SpecularShadingPass.SetSourceBuffer(albedoBuffer);
            m_SpecularShadingPass.SetTargetBuffer(pixelartCamera.GetBuffer(TargetBuffer.Specular));

            m_CombinationPass.SetSourceBuffer(albedoBuffer);
            m_CombinationPass.SetTargetBuffer(renderer.cameraColorTargetHandle);
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
                cmd.SetGlobalVector(ShaderPropertyStorage.ScreenParams, new Vector4(pixelartCamera.TargetWidth, pixelartCamera.TargetHeight, 1.0f + 1.0f / pixelartCamera.TargetWidth, 1.0f + 1.0f / pixelartCamera.TargetHeight));
                cmd.SetGlobalFloat(ShaderPropertyStorage.ConnectivityAntialiasingThreshold, m_AAScaler / 2.0f);
                cmd.SetGlobalInt(ShaderPropertyStorage.AdditionalLightCount, renderingData.lightData.additionalLightsCount);

                cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Diffuse), pixelartCamera.GetBuffer(TargetBuffer.Diffuse));
                cmd.SetGlobalTexture(TargetBufferUtil.GetBufferShaderProperty(TargetBuffer.Specular), pixelartCamera.GetBuffer(TargetBuffer.Specular));
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
