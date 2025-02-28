using GameServer;
using GameServer.Protocol;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    [RequireComponent(typeof(NetComponent))]
    public class SyncTransform : MonoBehaviour
    {
        public bool SyncRotation = true;
        public bool SyncScale = true;
        [Range(1, 60)]
        public int SyncFrames = 15;
        public bool IsFixedUpdate = false;
        public int CacheCount = 0;

        private Queue<TransformData> queueData = new Queue<TransformData>();
        private TransformData lastsyncData;
        private TransformData lastSendData;

        private float lastSendTime;
        private float syncTimer;
        private NetComponent netComponent;

        private bool startCache;

        private void Start()
        {
            netComponent = GetComponent<NetComponent>();

            startCache = true;
        }

        public void EnqueueData(TransformData data)
        {
            queueData.Enqueue(data);

            if (queueData.Count > CacheCount * 2 + 10)
            {
                Debug.LogWarning("queueData 缓存警告：" + queueData.Count);
            }
        }

        private void FixedUpdate()
        {
            if (!IsFixedUpdate)
                return;

            if (netComponent == null)
                return;

            if (netComponent.IsLocal)
            {
                SendTransformData(Time.fixedUnscaledDeltaTime);
            }
            else
            {
                SyncTransformData(Time.fixedUnscaledDeltaTime);
            }
        }

        private void LateUpdate()
        {
            if (IsFixedUpdate)
                return;

            if (netComponent == null)
                return;

            if (netComponent.IsLocal)
            {
                SendTransformData(Time.unscaledDeltaTime);
            }
            else
            {
                SyncTransformData(Time.unscaledDeltaTime);
            }
        }

        private void SendTransformData(float deltaTime)
        {
            if (lastSendData != null)
            {
                if (Mathf.Abs(transform.position.sqrMagnitude - lastSendData.Position.sqrMagnitude) < 0.0001f)
                {
                    if (Mathf.Abs(transform.localEulerAngles.sqrMagnitude - lastSendData.EulerAngles.sqrMagnitude) < 0.0001f)
                    {
                        if (Mathf.Abs(transform.localScale.sqrMagnitude - lastSendData.Scale.sqrMagnitude) < 0.0001f)
                        {
                            return;
                        }
                    }
                }
            }

            if ((Time.time - lastSendTime) < (1f / SyncFrames))
            {
                return;
            }

            lastSendTime = Time.time;

            SyncRequest syncRequest = new SyncRequest();
            syncRequest.SyncCode = SyncCode.SyncTransform;

            TransformData transformData = new TransformData();
            transformData.ObjectID = netComponent.ObjectID;
            transformData.Position = transform.position;

            if (SyncRotation)
                transformData.EulerAngles = transform.eulerAngles;
            else
                transformData.EulerAngles = Vector3.zero;
            if (SyncScale)
                transformData.Scale = transform.localScale;
            else
                transformData.Scale = Vector3.one;

            syncRequest.SyncData = MessagePackSerializer.Serialize(transformData);

            NetManager.Instance.SendRequest(OperationCode.SyncEvent, syncRequest, LiteNetLib.DeliveryMethod.Sequenced);

            lastSendData = transformData;
        }

        private void SyncTransformData(float deltaTime)
        {
            if (startCache && queueData.Count < CacheCount)
            {
                return;
            }

            if (queueData.Count <= 0)
            {
                lastsyncData = null;
                syncTimer = 0;
                return;
            }

            startCache = false;

            var curSyncData = queueData.Peek();

            if (lastsyncData == null)
            {
                lastsyncData = curSyncData;
            }

            syncTimer += deltaTime;

            float syncInterval = 1f / SyncFrames;

            float process = syncTimer / syncInterval;

            transform.position = Vector3.LerpUnclamped(lastsyncData.Position, curSyncData.Position, process);

            if (SyncRotation)
                transform.rotation = Quaternion.Lerp(Quaternion.Euler(lastsyncData.EulerAngles), Quaternion.Euler(curSyncData.EulerAngles), process);
            if (SyncScale)
                transform.localScale = Vector3.Lerp(lastsyncData.Scale, curSyncData.Scale, process);

            if (process >= 1f)
            {
                lastsyncData = queueData.Dequeue();

                syncTimer -= syncInterval;

                if (queueData.Count <= 0)
                {
                    startCache = true;
                }
            }
        }
    }
}