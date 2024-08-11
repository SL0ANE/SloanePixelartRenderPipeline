using UnityEngine;

namespace Sloane
{
    public static class ShaderPropertyStorage
    {
        public static readonly int ViewMatrix = Shader.PropertyToID("unity_MatrixV");
        public static readonly int InvViewMatrix = Shader.PropertyToID("unity_MatrixInvV");
        public static readonly int CameraViewMatrix = Shader.PropertyToID("_CameraMatrixV");
        public static readonly int CameraInvViewMatrix = Shader.PropertyToID("_CameraMatrixInvV");
        public static readonly int CameraViewProjectionMatrix = Shader.PropertyToID("_CameraMatrixVP");
        public static readonly int CameraInvViewProjectionMatrix = Shader.PropertyToID("_CameraMatrixInvVP");
        public static readonly int UnitSize = Shader.PropertyToID("_UnitSize");
        public static readonly int Width = Shader.PropertyToID("_Width");
        public static readonly int Height = Shader.PropertyToID("_Height");
        public static readonly int ScreenParams = Shader.PropertyToID("_ScreenParams");
        public static readonly int SamplingScale = Shader.PropertyToID("_SamplingScale");
        public static readonly int Target = Shader.PropertyToID("_Target");
        public static readonly int Source = Shader.PropertyToID("_Source");
        public static readonly int AdditionalLightCount = Shader.PropertyToID("_AdditionalLightCount");
        public static readonly int ConnectivityAntialiasingThreshold = Shader.PropertyToID("_ConnectivityAntialiasingThreshold");
        public static readonly int Threshold = Shader.PropertyToID("_Threshold");
        public static readonly int Palette = Shader.PropertyToID("_Palette");
        public static readonly int Count = Shader.PropertyToID("_Count");
        public static readonly int Resolution = Shader.PropertyToID("_Resolution");
        public static readonly int OutputBuffer = Shader.PropertyToID("_OutputBuffer");
        public static readonly int MainTex = Shader.PropertyToID("_MainTex");

        public static readonly int ConnectivityMap = Shader.PropertyToID("_ConnectivityMap");
        public static readonly int PrevConnectivityMap = Shader.PropertyToID("_PrevConnectivityMap");
    }
}