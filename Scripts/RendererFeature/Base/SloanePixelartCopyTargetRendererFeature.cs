using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Sloane
{
    public abstract class CameraTargetRendererFeaturePass : ScriptableRenderPass
    {
        protected RTHandle m_CameraColorTarget;
        protected RTHandle m_CameraDepthTarget;
        public void SetTarget(RTHandle colorHandle, RTHandle depthHandle)
        {
            m_CameraColorTarget = colorHandle;
            m_CameraDepthTarget = depthHandle;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(m_CameraColorTarget);
        }
    }
}
