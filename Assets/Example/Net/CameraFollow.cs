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
        public Vector3 angles;
        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }
            transform.localEulerAngles = angles;

            Vector3 desiredPosition = target.position + transform.right * offset.x + transform.up * offset.y + transform.forward * offset.z; // 计算期望的摄像机位置

            transform.position = Vector3.LerpUnclamped(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
