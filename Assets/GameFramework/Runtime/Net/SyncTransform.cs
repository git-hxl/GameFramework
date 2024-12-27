using GameServer.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class SyncTransform : MonoBehaviour
    {
        [SerializeField]
        private Vector3 targetPosition;
        [SerializeField]
        private Quaternion targetRotation;
        [SerializeField]
        private float speed;

        [SerializeField]
        private int bufferNumber;

        private Queue<SyncTransformData> syncTransformDatas = new Queue<SyncTransformData>();

        private GameObject copy;
        public void SetTargetTransform(SyncTransformData syncTransformData)
        {
            targetPosition = syncTransformData.Position;
            targetRotation = syncTransformData.Rotation;
            speed = syncTransformData.Speed;

            syncTransformDatas.Enqueue(syncTransformData);

            float offset = Vector3.Distance(transform.position, targetPosition);

            Debug.Log(name + " Sync offset:" + offset);

            copy.transform.position = targetPosition;

            copy.transform.rotation = targetRotation;

            bufferNumber = syncTransformDatas.Count;
        }

        // Start is called before the first frame update
        void Start()
        {
            copy = new GameObject(name+"_Copy");
            
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (syncTransformDatas.Count >= 2)
            {
                SyncTransformData syncTransformData = syncTransformDatas.Peek();

                float distance = Vector3.Distance(transform.position, syncTransformData.Position);

                float delta = speed * Time.deltaTime;

                if (distance > delta)
                {
                    Vector3 moveDir = (syncTransformData.Position - transform.position).normalized;
                    transform.position += moveDir * delta;

                    transform.rotation = Quaternion.Lerp(transform.rotation, syncTransformData.Rotation, speed * Time.deltaTime);
                }
                else
                {
                    syncTransformDatas.Dequeue();
                }
            }
        }
    }
}
