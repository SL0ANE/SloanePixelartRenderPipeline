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
        ShapeProperty = 4,    // 优先级，法线融合强度, 法线边缘阈值
        PaletteProperty = 5,    // 主光源级数，dither灰度
        RimLightProperty = 6,    // 边缘光颜色与强度
        UV = 7,    // 根据优先级整出的UV偏移
        ConnectivityDetail = 8,
        ConnectivityResult = 9,
        Diffuse = 10,
        Specular = 11,
        GlobalIllumination = 12,
        RimLight = 13,
        Max,
    }

    // 记录每个阶段的结尾Buffer索引
    public enum TargetBufferStage
    {
        Start = -1,
        MarkerDepth = 0,
        StageRenderObjects = 6,
        MarkerPriority = 7,
        MarkerConnectivityDetail = 8,
        MarkerConnectivityResult = 9,
        StageShading = 13,

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