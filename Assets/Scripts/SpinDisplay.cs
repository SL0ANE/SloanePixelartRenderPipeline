using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinDisplay : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10.0f; // 旋转速度，可以在 Unity 编辑器中调整
    [SerializeField]
    private float rotationRange = 30.0f; // 旋转范围，可以在 Unity 编辑器中调整
    [SerializeField]
    private float interval = 0.16666f; // 时间间隔，可以在 Unity 编辑器中调整

    private float time;
    private float elapsedTime;

    void Update()
    {
        // 更新时间
        elapsedTime += Time.deltaTime;

        // 检查是否达到时间间隔
        if (elapsedTime >= interval)
        {
            // 重置时间
            elapsedTime = 0;

            // 更新旋转时间
            time += interval * rotationSpeed;

            // 计算正弦旋转角度
            float angle = Mathf.Sin(time) * rotationRange;

            // 应用旋转
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
}