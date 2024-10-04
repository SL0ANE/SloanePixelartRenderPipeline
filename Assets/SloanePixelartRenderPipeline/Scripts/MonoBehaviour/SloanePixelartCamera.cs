using System.Collections;
using System.Collections.Generic;
using Sloane;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Sloane
{
    [ExecuteAlways, RequireComponent(typeof(Camera))]
    public class SloanePixelartCamera : MonoBehaviour
    {
        protected static Dictionary<Camera, SloanePixelartCamera> m_CameraMap = new Dictionary<Camera, SloanePixelartCamera>();
        public static Dictionary<Camera, SloanePixelartCamera> CameraMap => m_CameraMap;
        protected static Dictionary<Camera, SloanePixelartCamera> m_CastCameraMap = new Dictionary<Camera, SloanePixelartCamera>();
        public static Dictionary<Camera, SloanePixelartCamera> CastCameraMap => m_CastCameraMap;

        [SerializeField]
        Vector2Int m_TargetResolution = new Vector2Int(320, 180);
        [SerializeField]
        int m_DownSamplingScale = 5;
        [SerializeField, HideInInspector]
        SloanePixelartCastCamera m_CastCamera;
        [SerializeField, HideInInspector]
        Camera m_ThisCamera;
        [SerializeField, HideInInspector]
        UniversalAdditionalCameraData m_CastCameraData;
        [SerializeField, HideInInspector]
        UniversalAdditionalCameraData m_ThisCameraData;
        [SerializeField, HideInInspector]
        RenderTexture m_ResultBuffer;
        [SerializeField, HideInInspector]
        List<RenderTexture> m_TargetBuffers;
        RenderTargetIdentifier[] m_OpaqueBuffersIdentifiers = new RenderTargetIdentifier[TargetBufferStage.StageRenderObjects - TargetBufferStage.MarkerDepth];
        RenderTargetIdentifier[] m_ShadingBuffersIdentifiers = new RenderTargetIdentifier[TargetBufferStage.StageShading - TargetBufferStage.MarkerConnectivityResult];

        public Vector2Int TextureResolution => m_TargetResolution * m_DownSamplingScale;
        public int TextureWidth => m_TargetResolution.x * m_DownSamplingScale;
        public int TextureHeight => m_TargetResolution.y * m_DownSamplingScale;
        public int TargetWidth => m_TargetResolution.x;
        public int TargetHeight => m_TargetResolution.y;
        public int DownSamplingScale => m_DownSamplingScale;
        public RenderTexture ResultBuffer => m_ResultBuffer;
        public float UnitSize => m_CastCamera.Camera.orthographicSize * 2.0f / TargetHeight;

        public RenderTargetIdentifier[] OpaqueBuffersIdentifiers => m_OpaqueBuffersIdentifiers;
        public RenderTargetIdentifier[] ShadingBuffersIdentifiers => m_ShadingBuffersIdentifiers;

        public enum CameraTarget
        {
            MainCamera,
            CastCamera
        }

        public static SloanePixelartCamera GetPixelartCamera(Camera camera, CameraTarget target = CameraTarget.MainCamera)
        {
            var pixelArtCamera = target == CameraTarget.MainCamera ? (CameraMap.ContainsKey(camera) ? CameraMap[camera] : null) : (CastCameraMap.ContainsKey(camera) ? CastCameraMap[camera] : null);

            if (pixelArtCamera == null) Debug.LogWarning($"{SloanePixelartGlobalConst.LogHeader} 相机({camera.gameObject.name})应当有对应的像素画相机组件！");

            return pixelArtCamera;
        }

        public RenderTexture GetBuffer(TargetBuffer targetBuffer)
        {
            int index = (int)targetBuffer;
            if (index >= m_TargetBuffers.Count || m_TargetBuffers == null) return m_ResultBuffer;
            return m_TargetBuffers[index];
        }

        void OnEnable()
        {
            Initialize();
            GetBuffers();

            m_CameraMap.Add(m_ThisCamera, this);
            m_CastCameraMap.Add(m_CastCamera.Camera, this);
            m_CastCamera.enabled = true;
        }

        void OnDisable()
        {
            ReleaseBuffers();

            m_CameraMap.Remove(m_ThisCamera);
            m_CastCameraMap.Remove(m_CastCamera.Camera);
            m_CastCamera.enabled = false;
        }

        void OnValidate()
        {
            GetBuffers();
        }

        private void Initialize()
        {
            InitializeThisCamera();
            InitializeCastCamera();
        }

        private void InitializeThisCamera()
        {
            if (m_ThisCamera == null)
            {
                m_ThisCamera = GetComponent<Camera>();
                m_ThisCameraData = m_ThisCamera.GetComponent<UniversalAdditionalCameraData>();
                if (m_ThisCameraData == null) m_ThisCameraData = m_ThisCamera.AddComponent<UniversalAdditionalCameraData>();
            }

            m_ThisCamera.clearFlags = CameraClearFlags.Nothing;
            m_ThisCamera.cullingMask = 0;
            m_ThisCamera.farClipPlane = 0.02f;
            m_ThisCamera.nearClipPlane = 0.01f;

            m_ThisCameraData.SetRenderer((int)SloanePixelartRenderer.MainCamera);
        }

        private void ReleaseBuffers()
        {
            if (m_TargetBuffers != null)
            {
                foreach (var buffer in m_TargetBuffers)
                {
                    if (m_TargetBuffers != null)
                    {
                        RenderTexture.ReleaseTemporary(buffer);
                        m_TargetBuffers = null;
                    }
                }
            }

            if (m_ResultBuffer != null)
            {
                m_CastCamera.Camera.targetTexture = null;
                RenderTexture.ReleaseTemporary(m_ResultBuffer);
                m_ResultBuffer = null;
            }
        }

        /* private void UpdateParam()
        {
            
        } */

        private void GetBuffers()
        {
            ReleaseBuffers();

            if (m_TargetBuffers == null) m_TargetBuffers = new List<RenderTexture>();
            else m_TargetBuffers?.Clear();

            RenderTextureDescriptor targetDesc = new RenderTextureDescriptor(TextureWidth, TextureHeight)
            {
                depthBufferBits = 24,
                graphicsFormat = GraphicsFormat.None,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D
            };

            for (int i = (int)TargetBufferStage.Start + 1; i <= (int)TargetBufferStage.MarkerDepth; i++)
            {
                m_TargetBuffers.Add(RenderTexture.GetTemporary(targetDesc));
                m_TargetBuffers[i].filterMode = FilterMode.Point;
                m_TargetBuffers[i].Create();
            }

            targetDesc = new RenderTextureDescriptor(TextureWidth, TextureHeight)
            {
                depthBufferBits = 0,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16G16B16A16_SNorm,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D
            };

            for (int i = (int)TargetBufferStage.MarkerDepth + 1; i <= (int)TargetBufferStage.StageRenderObjects; i++)
            {
                m_TargetBuffers.Add(RenderTexture.GetTemporary(targetDesc));
                m_TargetBuffers[i].filterMode = FilterMode.Point;
                m_TargetBuffers[i].Create();
                m_OpaqueBuffersIdentifiers[i - (int)TargetBufferStage.MarkerDepth - 1] = m_TargetBuffers[i];
            }

            for (int i = (int)TargetBufferStage.StageRenderObjects + 1; i <= (int)TargetBufferStage.StagePostBeforeDownSampling; i++)
            {
                m_TargetBuffers.Add(RenderTexture.GetTemporary(targetDesc));
                m_TargetBuffers[i].filterMode = FilterMode.Point;
                m_TargetBuffers[i].Create();
            }

            targetDesc = new RenderTextureDescriptor(TextureWidth, TextureHeight)
            {
                depthBufferBits = 0,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R8G8B8A8_SNorm,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D
            };

            for (int i = (int)TargetBufferStage.StagePostBeforeDownSampling + 1; i <= (int)TargetBufferStage.MarkerConnectivityDetail; i++)
            {
                m_TargetBuffers.Add(RenderTexture.GetTemporary(targetDesc));
                m_TargetBuffers[i].filterMode = FilterMode.Point;
                m_TargetBuffers[i].Create();
            }

            targetDesc = new RenderTextureDescriptor(TargetWidth, TargetHeight)
            {
                depthBufferBits = 0,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D
            };

            for (int i = (int)TargetBufferStage.MarkerConnectivityDetail + 1; i <= (int)TargetBufferStage.MarkerConnectivityResult; i++)
            {
                m_TargetBuffers.Add(RenderTexture.GetTemporary(targetDesc));
                m_TargetBuffers[i].filterMode = FilterMode.Point;
                m_TargetBuffers[i].Create();
            }

            targetDesc = new RenderTextureDescriptor(TargetWidth, TargetHeight)
            {
                depthBufferBits = 0,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R16G16B16A16_SNorm,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D
            };

            for (int i = (int)TargetBufferStage.MarkerConnectivityResult + 1; i <= (int)TargetBufferStage.StageShading; i++)
            {
                m_TargetBuffers.Add(RenderTexture.GetTemporary(targetDesc));
                m_TargetBuffers[i].filterMode = FilterMode.Point;
                m_TargetBuffers[i].Create();

                m_ShadingBuffersIdentifiers[i - (int)TargetBufferStage.MarkerConnectivityResult - 1] = m_TargetBuffers[i];
            }

            RenderTextureDescriptor resultDesc = new RenderTextureDescriptor(TextureWidth, TextureHeight)
            {
                depthBufferBits = 24,
                enableRandomWrite = true,
                graphicsFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR),
                sRGB = true,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D
            };

            m_ResultBuffer = RenderTexture.GetTemporary(resultDesc);
            m_ResultBuffer.filterMode = FilterMode.Point;
            m_ResultBuffer.Create();

            m_CastCamera.Camera.targetTexture = m_ResultBuffer;
        }

        private void InitializeCastCamera()
        {
            if (m_CastCamera == null)
            {
                GameObject castCameraObject = new GameObject("Cast Camera");
                castCameraObject.transform.SetParent(transform, false);
                castCameraObject.AddComponent<Camera>();
                m_CastCamera = castCameraObject.AddComponent<SloanePixelartCastCamera>();
                m_CastCamera.Initialize();
                m_CastCameraData = m_CastCamera.GetComponent<UniversalAdditionalCameraData>();
                if (m_CastCameraData == null) m_CastCameraData = m_CastCamera.AddComponent<UniversalAdditionalCameraData>();
                m_CastCamera.Camera.orthographicSize = 6.125f;    // Celeste
            }

            m_CastCamera.Camera.orthographic = true;
            m_CastCamera.Camera.depth = -64;
            // m_CastCamera.enabled = false;
            m_CastCameraData.SetRenderer((int)SloanePixelartRenderer.CastCamera);
        }
    }
}
