using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Sloane
{
    [Serializable, CreateAssetMenu(fileName = "New Palette", menuName = "Sloane Pixelart/Palette", order = 1)]
    public class SloanePixelartPalette : ScriptableObject
    {
        public List<Color> Colors = new List<Color>();
        [SerializeField]
        private ComputeShader m_BakeComputeShader;
        private static ArrayPool<Vector3> m_ArrayPool = ArrayPool<Vector3>.Create();

        public Texture2D GenerateTexture2D(int resolution = 16)
        {
            RenderTextureDescriptor targetDesc = new RenderTextureDescriptor(resolution * resolution, resolution)
            {
                depthBufferBits = 0,
                enableRandomWrite = true,
                graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D
            };

            var targetRT = RenderTexture.GetTemporary(targetDesc);
            if(!GenerateRenderTexture(targetRT)) return null;

            Texture2D outputTexture = new Texture2D(targetRT.width, targetRT.height, TextureFormat.RGBA32, false);

            RenderTexture.active = targetRT;
            outputTexture.ReadPixels(new Rect(0, 0, targetRT.width, targetRT.height), 0, 0);
            RenderTexture.active = null;

            RenderTexture.ReleaseTemporary(targetRT);

            return outputTexture;
        }

        public bool GenerateRenderTexture(RenderTexture targetRT)
        {
            if (targetRT.width != targetRT.height * targetRT.height || m_BakeComputeShader == null) return false;
            int shaderKernel = m_BakeComputeShader.FindKernel("Main");

            m_BakeComputeShader.SetInt(ShaderPropertyStorage.Resolution, targetRT.height);
            m_BakeComputeShader.SetTexture(shaderKernel, ShaderPropertyStorage.OutputBuffer, targetRT);
            
            Vector3[] colors = m_ArrayPool.Rent(Colors.Count);
            for(int i = 0; i < Colors.Count; i++)
            {
                colors[i] = new Vector3(Colors[i].r, Colors[i].g, Colors[i].b);
            }
            ComputeBuffer paletteBuffer = new ComputeBuffer(Colors.Count, sizeof(float) * 3);
            paletteBuffer.SetData(colors, 0, 0, Colors.Count);

            m_BakeComputeShader.SetBuffer(shaderKernel,ShaderPropertyStorage.Palette, paletteBuffer);
            m_BakeComputeShader.SetInt(ShaderPropertyStorage.Count, Colors.Count);
            int dispatchCount = Mathf.CeilToInt((float)targetRT.height / 8);
            m_BakeComputeShader.Dispatch(shaderKernel, dispatchCount, dispatchCount, dispatchCount);

            m_ArrayPool.Return(colors);
            paletteBuffer.Release();
            return true;
        }
    }
}