using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinDisplay : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10.0f; // 旋转速度，可以在 Unity 编辑器中调整
    [SerializeField]
    private bool m_ApplyRange = false; // 是否使用正弦旋转，可以在 Unity 编辑器中调整
    [SerializeField]
    private float rotationRange = 30.0f; // 旋转范围，可以在 Unity 编辑器中调整
    [SerializeField]
    private float interval = 0.16666f; // 时间间隔，可以在 Unity 编辑器中调整
    private Quaternion m_OriginalRotation;
    private float time;
    private float elapsedTime;

    void Start()
    {
        m_OriginalRotation = transform.rotation;
    }

    void Update()
    {
        // 更新时间
        elapsedTime += Time.deltaTime;

        // 检查是否达到时间间隔
        if (elapsedTime >= interval)
        {
            elapsedTime = 0;
            time += interval;
            float angle = 0;

            if (m_ApplyRange)
            {
                // 使用正弦旋转
                angle = Mathf.Sin(time * rotationSpeed) * rotationRange;
            }
            else
            {
                float period = 2 * Mathf.PI / rotationSpeed;
                float phase = time % period / period;
                angle = Mathf.FloorToInt(phase * 2) % 2 == 0 ? Mathf.Cos(phase * 2.0f * Mathf.PI) : -Mathf.Cos(phase * 2.0f * Mathf.PI);
                angle = angle * 0.5f + 0.5f;
                angle = angle * 360.0f;
            }

            transform.rotation = Quaternion.Euler(0, angle, 0) * m_OriginalRotation;
        }
    }

    /* void OnValidate()
    {
        Debug.Log($"{gameObject.name} {transform.localToWorldMatrix}");
    } */
}