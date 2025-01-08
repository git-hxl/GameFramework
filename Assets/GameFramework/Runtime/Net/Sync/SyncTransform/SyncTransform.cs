using GameServer;
using GameServer.Protocol;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class SyncTransform : MonoBehaviour
    {
        public bool SyncRotation = true;
        public bool SyncScale = true;
        [Range(1, 30)]
        public int SyncFrames = 15;
        public int CacheCount = 2;

        public bool IsClient { get; private set; }

        private Queue<TransformSnapshot> transformSnapshots = new Queue<TransformSnapshot>();

        private TransformSnapshot lastSyncSnapshot;

        private SyncTransformData lastSendData;

        private float sendTimer;
        private float syncTimer;

        private void Start()
        {

        }

        public void Init(int id)
        {
            IsClient = NetManager.Instance.ID == id;
        }

        public void AddSnapshot(long timestamp, SyncTransformData data)
        {
            TransformSnapshot transformSnapshoot = ReferencePool.Instance.Acquire<TransformSnapshot>();
            transformSnapshoot.Timestamp = timestamp;
            transformSnapshoot.TransformData = data;

            transformSnapshots.Enqueue(transformSnapshoot);

            if (transformSnapshots.Count > CacheCount * 2)
            {
                Debug.LogWarning("TransformSnapshoots 缓存警告：" + transformSnapshots.Count);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (IsClient)
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

            if (lastSendData != null)
            {
                float distance = Vector3.Distance(transform.position, lastSendData.Position);
                float angle = Vector3.Angle(transform.forward, lastSendData.Direction);

                if (distance < 0.1f && angle < 1f)
                {
                    return;
                }
            }
            else
            {
                lastSendData = new SyncTransformData();
            }

            SyncTransformData syncTransformData = lastSendData;
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
            if (transformSnapshots.Count <= 0)
            {
                return;
            }

            var curSnapshot = transformSnapshots.Peek();

            if (lastSyncSnapshot == null)
            {
                lastSyncSnapshot = curSnapshot;
            }

            syncTimer += Time.deltaTime;

            float delta = syncTimer / (1f / SyncFrames);

            transform.position = Vector3.Lerp(lastSyncSnapshot.TransformData.Position, curSnapshot.TransformData.Position, delta);

            if (SyncRotation)
                transform.forward = Vector3.Lerp(lastSyncSnapshot.TransformData.Direction, curSnapshot.TransformData.Direction, delta);
            if (SyncScale)
                transform.localScale = Vector3.Lerp(lastSyncSnapshot.TransformData.Scale, curSnapshot.TransformData.Scale, delta);

            if (delta >= 1f)
            {
                lastSyncSnapshot = transformSnapshots.Dequeue();

                syncTimer = 0;
            }
        }
    }
}