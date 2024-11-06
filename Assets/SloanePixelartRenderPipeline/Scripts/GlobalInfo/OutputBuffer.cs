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
        Normal0 = 2,
        Normal1 = 3,
        PhysicalProperty = 4,   // 光滑度，金属度
        ShapeProperty = 5,    // 优先级，法线融合强度（待删）, 法线边缘阈值
        PaletteProperty = 6,    // 主光源级数，dither灰度, 边缘增减级数, 布尔信息（0：是否应用描边）
        RimLightProperty = 7,    // 边缘光颜色与强度
        UV = 8,    // 根据优先级整出的UV偏移
        ConnectivityDetail = 9,
        ConnectivityResult = 10,
        Diffuse = 11,
        Specular = 12,
        GlobalIllumination = 13,
        RimLight = 14,
        Max,
    }

    // 记录每个阶段的结尾Buffer索引
    public enum TargetBufferStage
    {
        Start = -1,
        MarkerDepth = 0,
        StageRenderObjects = 7,
        MarkerPriority = 8,
        MarkerConnectivityDetail = 9,
        MarkerConnectivityResult = 10,
        StageShading = 14,

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