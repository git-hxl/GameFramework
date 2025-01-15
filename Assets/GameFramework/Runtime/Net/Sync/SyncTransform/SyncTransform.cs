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
        [Range(1, 30)]
        public int SyncFrames = 15;

        private Queue<TransformSnapshot> transformSnapshots = new Queue<TransformSnapshot>();

        private TransformSnapshot lastSyncSnapshot;

        private SyncTransformData lastSendData;

        private float sendTimer;
        private float syncTimer;
        private NetComponent netComponent;

        public long Timestamp { get; private set; }
        private void Start()
        {
            netComponent = GetComponent<NetComponent>();
        }

        public void AddSnapshot(long timestamp, SyncTransformData data)
        {
            //Debug.Log("处理事件时间：" + timestamp);

            TransformSnapshot transformSnapshoot = ReferencePool.Instance.Acquire<TransformSnapshot>();
            transformSnapshoot.Timestamp = timestamp;
            transformSnapshoot.TransformData = data;

            transformSnapshots.Enqueue(transformSnapshoot);

            if (transformSnapshots.Count > 10)
            {
                Debug.LogWarning("TransformSnapshoots 缓存警告：" + transformSnapshots.Count);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (netComponent == null)
                return;

            if (netComponent.IsLocal)
            {
                SendTransformData();
            }
            else
            {
                SyncTransformData();
            }
        }

        private void SendTransformData()
        {
            sendTimer += Time.unscaledDeltaTime;
            if (sendTimer < 1f / SyncFrames)
            {
                return;
            }
            sendTimer = 0;

            if (lastSendData == null)
            {
                lastSendData = new SyncTransformData();
            }
            else
            {
                float distance = Vector3.Distance(transform.position, lastSendData.Position);
                float angle = Vector3.Angle(transform.forward, lastSendData.Direction);

                if (distance < 0.0001f && angle < 0.01f)
                {
                    return;
                }
            }
            SyncTransformData syncTransformData = lastSendData;
            syncTransformData.ObjectID = netComponent.ObjectID;
            syncTransformData.Position = transform.position;
            if (SyncRotation)
                syncTransformData.Direction = transform.forward;
            if (SyncScale)
                syncTransformData.Scale = transform.localScale;
            byte[] data = MessagePackSerializer.Serialize(syncTransformData);

            NetManager.Instance.SendSyncEvent(SyncEventCode.SyncTransform, data, LiteNetLib.DeliveryMethod.Sequenced);
        }

        private void SyncTransformData()
        {
            Timestamp += (int)(Time.unscaledDeltaTime * 1000);

            if (transformSnapshots.Count <= 0)
            {
                return;
            }

            syncTimer += Time.unscaledDeltaTime;

            var curSnapshot = transformSnapshots.Peek();

            if (lastSyncSnapshot == null)
            {
                lastSyncSnapshot = curSnapshot;

                Timestamp = lastSyncSnapshot.Timestamp;
            }

            float syncInterval = 1f / SyncFrames;

            float delta = syncTimer / syncInterval;

            transform.position = Vector3.Lerp(lastSyncSnapshot.TransformData.Position, curSnapshot.TransformData.Position, delta);

            if (SyncRotation)
                transform.forward = Vector3.Lerp(lastSyncSnapshot.TransformData.Direction, curSnapshot.TransformData.Direction, delta);
            if (SyncScale)
                transform.localScale = Vector3.Lerp(lastSyncSnapshot.TransformData.Scale, curSnapshot.TransformData.Scale, delta);

            if (delta >= 1f)
            {
                if (lastSyncSnapshot != null && lastSyncSnapshot != curSnapshot)
                {
                    ReferencePool.Instance.Release(lastSyncSnapshot);
                }
                lastSyncSnapshot = transformSnapshots.Dequeue();
                syncTimer -= syncInterval;
                Timestamp = lastSyncSnapshot.Timestamp;
            }
        }
    }
}