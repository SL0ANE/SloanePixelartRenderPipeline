using UnityEngine;

namespace Sloane
{
    public static class SloanePixelartShaderPropertyStorage
    {
        public static readonly int ViewMatrix = Shader.PropertyToID("unity_MatrixV");
        public static readonly int UnitSize = Shader.PropertyToID("_UnitSize");
        public static readonly int Width = Shader.PropertyToID("_Width");
        public static readonly int Height = Shader.PropertyToID("_Height");
        public static readonly int SamplingScale = Shader.PropertyToID("_SamplingScale");
        public static readonly int Target = Shader.PropertyToID("_Target");
        public static readonly int Source = Shader.PropertyToID("_Source");
    }
}