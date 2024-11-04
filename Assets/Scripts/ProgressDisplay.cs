using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressDisplay : MonoBehaviour
{
    [SerializeField]
    private float cycleDuration = 2.0f;
    [SerializeField]
    private float cycleOffset = 0.0f;

    private MeshRenderer meshRenderer;
    private Material material;
    private float elapsedTime;

    void Start()
    {

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // 获取材质球
            material = meshRenderer.material;
        }
    }

    void Update()
    {
        if (material != null)
        {
            // 更新时间
            elapsedTime += Time.deltaTime;

            // 计算当前时间在一个周期中的位置
            float t = ((elapsedTime + cycleOffset) % cycleDuration) / cycleDuration;

            // 使用正弦函数计算 _Progress 值，范围在 0 到 1 之间
            float progress = Mathf.Lerp(-0.05f, 1.05f, (Mathf.Sin(t * Mathf.PI * 2.0f) + 1.0f) / 2.0f);

            // 设置材质球上的 _Progress 属性
            material.SetFloat("_Progress", progress);
        }
    }
}