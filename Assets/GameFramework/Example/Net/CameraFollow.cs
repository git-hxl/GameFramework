using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target; // 要跟随的目标对象
        public float smoothSpeed = 0.125f; // 平滑速度
        public Vector3 offset; // 摄像机与目标对象的偏移量

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }
            Vector3 desiredPosition = target.position + offset; // 计算期望的摄像机位置
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed); // 使用线性插值实现平滑过渡
            transform.position = smoothedPosition; // 更新摄像机位置
        }
    }
}
