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

        public float SendInterval = 0.1f;
        public bool IsFixedUpdate = false;

        private Queue<TransformData> queueData = new Queue<TransformData>();
        private TransformData lastsyncData;
        private TransformData lastSendData;

        private float lastSendTime;
        private float syncTimer;
        private NetComponent netComponent;

        private void Start()
        {
            netComponent = GetComponent<NetComponent>();
        }

        public void EnqueueData(TransformData data)
        {
            queueData.Enqueue(data);
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
            if ((Time.time - lastSendTime) < SendInterval)
            {
                return;
            }

            lastSendTime = Time.time;

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

            SyncRequest syncRequest = new SyncRequest();
            syncRequest.SyncCode = SyncCode.SyncTransform;

            var transformData = GetTransformData();

            syncRequest.SyncData = MessagePackSerializer.Serialize(transformData);

            NetManager.Instance.SendRequest(OperationCode.SyncEvent, syncRequest, LiteNetLib.DeliveryMethod.Sequenced);

            lastSendData = transformData;
        }

        private void SyncTransformData(float deltaTime)
        {
            if (queueData.Count <= 0)
            {
                syncTimer = 0;
                return;
            }

            var curSyncData = queueData.Peek();


            syncTimer += deltaTime;


            if (lastsyncData == null)
            {
                lastsyncData = queueData.Dequeue();

                return;
            }

            float syncInterval = curSyncData.Time - lastsyncData.Time;

            if (syncInterval <= 0 || syncInterval > SendInterval * 2)
                syncInterval = SendInterval;

            float syncProcess = syncTimer / syncInterval;


            Debug.Log(string.Format("SyncTransform syncTimer {0} syncInterval {1} syncProcess {2}", syncTimer, syncInterval, syncProcess));

            transform.position = Vector3.LerpUnclamped(lastsyncData.Position, curSyncData.Position, syncProcess);

            if (SyncRotation)
                transform.rotation = Quaternion.Lerp(Quaternion.Euler(lastsyncData.EulerAngles), Quaternion.Euler(curSyncData.EulerAngles), syncProcess);
            if (SyncScale)
                transform.localScale = Vector3.Lerp(lastsyncData.Scale, curSyncData.Scale, syncProcess);

            if (syncProcess >= 1f)
            {
                lastsyncData = queueData.Dequeue();

                syncTimer -= syncInterval;
            }
        }

        private TransformData GetTransformData()
        {
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

            transformData.Time = Time.time;

            return transformData;
        }

    }
}