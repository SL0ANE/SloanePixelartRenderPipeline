using UnityEngine;

namespace Sloane
{
    public static class ShaderPropertyStorage
    {
        public static readonly int ViewMatrix = Shader.PropertyToID("unity_MatrixV");
        public static readonly int InvViewMatrix = Shader.PropertyToID("unity_MatrixInvV");
        public static readonly int UnitSize = Shader.PropertyToID("_UnitSize");
        public static readonly int Width = Shader.PropertyToID("_Width");
        public static readonly int Height = Shader.PropertyToID("_Height");
        public static readonly int SamplingScale = Shader.PropertyToID("_SamplingScale");
        public static readonly int Target = Shader.PropertyToID("_Target");
        public static readonly int Source = Shader.PropertyToID("_Source");
        public static readonly int AdditionalLightCount = Shader.PropertyToID("_AdditionalLightCount");
        public static readonly int Threshold = Shader.PropertyToID("_Threshold");

        public static readonly int ConnectivityMap = Shader.PropertyToID("_ConnectivityMap");
    }
}