using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sloane
{
    // Depth should be handled individually.
    public enum TargetBuffer
    {
        Depth = 0,
        Albedo = 1,
        Normal = 2,
        PhysicalProperty = 3,   // 光滑度，金属度
        ShapeProperty = 4,
        PaletteProperty = 5,    // 主光源级数，dither灰度
        UV = 6,
        ConnectivityDetail = 7,
        ConnectivityResult = 8,
        Diffuse = 9,
        Specular = 10,
        GlobalIllumination = 11,
        Max,
    }

    // 记录每个阶段的结尾Buffer索引
    public enum TargetBufferStage
    {
        Start = -1,
        MarkerDepth = 0,
        StageRenderObjects = 6,
        StagePostBeforeDownSampling = 6,
        MarkerConnectivityDetail = 7,
        MarkerConnectivityResult = 8,
        StageShading = 11,
        StagePostAfterDownSampling = 11,
        Max,
    }

    public static class TargetBufferUtil
    {
        private static List<int> m_TargetBufferShaderProperty;
        private static bool m_Initialize = false;

        private static void Initialize()
        {
            if(m_Initialize) return;
            m_Initialize = true;

            m_TargetBufferShaderProperty = new List<int>();
            for(int i = 0; i < (int)TargetBuffer.Max; i++)
            {
                string enumName = Enum.GetName(typeof(TargetBuffer), (TargetBuffer)i);
                m_TargetBufferShaderProperty.Add(Shader.PropertyToID($"_{enumName}Buffer"));
                // Debug.Log($"_{enumName}Buffer");
            }
        }

        public static int GetBufferShaderProperty(TargetBuffer targetBuffer)
        {
            Initialize();
            return m_TargetBufferShaderProperty[(int)targetBuffer];
        }
    }
}