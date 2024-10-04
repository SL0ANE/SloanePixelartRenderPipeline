using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinDisplay : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10.0f; // 旋转速度，可以在 Unity 编辑器中调整

    void Update()
    {
        // 每帧根据时间增量和设定的旋转速度旋转物体
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);
    }
}