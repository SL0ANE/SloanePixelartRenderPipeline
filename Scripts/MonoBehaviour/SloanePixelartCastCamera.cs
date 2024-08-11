using UnityEngine;
using UnityEngine.Rendering;

namespace Sloane
{
    [ExecuteAlways]
    public class SloanePixelartCastCamera : MonoBehaviour
    {
        static SloanePixelartCastCamera m_current;
        public static SloanePixelartCastCamera Current => m_current;
        [SerializeField, HideInInspector]
        private Camera m_Camera;
        public Camera Camera => m_Camera;

        public void Initialize()
        {
            if (m_Camera == null)
            {
                m_Camera = GetComponent<Camera>();
            }
        }

        void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }
        void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera != m_Camera)
            {
                m_current = null;
                return;
            }
            m_current = this;
        }
    }
}