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
        [Range(0.06f, 1f)]
        public float SyncInterval = 0.1f;

        public bool IsClient { get; private set; }

        private Queue<TransformSnapshoot> transformSnapshoots = new Queue<TransformSnapshoot>();

        private TransformSnapshoot lastSnapshoot;
        private TransformSnapshoot curSnapshoot;

        private float sendTimer;
        private float syncTimer;

        private int syncSpeed;
        private void Start()
        {

        }

        public void Init(int id)
        {
            IsClient = NetManager.Instance.PlayerID == id;
        }

        public void AddData(long timestamp, SyncTransformData data)
        {
            TransformSnapshoot transformSnapshoot = ReferencePool.Instance.Acquire<TransformSnapshoot>();

            transformSnapshoot.Timestamp = timestamp;
            transformSnapshoot.TransformData = data;

            transformSnapshoots.Enqueue(transformSnapshoot);

            if (transformSnapshoots.Count >= 10)
            {
                Debug.LogWarning("TransformSnapshoots 缓存警告：" + transformSnapshoots.Count);
            }

            if (curSnapshoot != null)
            {
                long offset = timestamp - curSnapshoot.Timestamp;

                syncSpeed = (int)(offset / 1000f / (SyncInterval / 2)) + 1;

                if (offset > 1000)
                {
                    Debug.LogWarning("TransformSnapshoots 延迟警告：" + offset);
                }
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
            if (sendTimer < SyncInterval)
            {
                return;
            }
            sendTimer = 0;

            SyncTransformData syncTransformData = new SyncTransformData();
            syncTransformData.Position = transform.position;
            if (SyncRotation)
                syncTransformData.Rotation = transform.rotation;
            if (SyncScale)
                syncTransformData.Scale = transform.localScale;
            byte[] data = MessagePackSerializer.Serialize(syncTransformData);

            NetManager.Instance.SendSyncEvent(SyncCode.SyncTransform, data, LiteNetLib.DeliveryMethod.Sequenced);
        }

        private void SyncTransformData()
        {
            if (transformSnapshoots.Count <= 0)
                return;

            if (lastSnapshoot == curSnapshoot || curSnapshoot == null)
            {
                curSnapshoot = transformSnapshoots.Dequeue();
            }

            if (curSnapshoot == null)
            {
                return;
            }

            if (lastSnapshoot == null)
            {
                lastSnapshoot = curSnapshoot;
            }

            syncTimer += Time.deltaTime;

            float delta = 0;

            if (curSnapshoot.Timestamp == lastSnapshoot.Timestamp)
            {
                delta = 1f;
            }

            delta = syncTimer / ((curSnapshoot.Timestamp - lastSnapshoot.Timestamp) / 1000f);

            transform.position = Vector3.Lerp(lastSnapshoot.TransformData.Position, curSnapshoot.TransformData.Position, delta);

            if (SyncRotation)
                transform.rotation = Quaternion.Lerp(lastSnapshoot.TransformData.Rotation, curSnapshoot.TransformData.Rotation, delta);
            if (SyncScale)
                transform.localScale = Vector3.Lerp(lastSnapshoot.TransformData.Scale, curSnapshoot.TransformData.Scale, delta);

            if (delta >= 1f)
            {
                if (lastSnapshoot != null && lastSnapshoot != curSnapshoot)
                {
                    ReferencePool.Instance.Release(lastSnapshoot);
                }

                lastSnapshoot = curSnapshoot;

                syncTimer = 0;
            }
        }
    }
}