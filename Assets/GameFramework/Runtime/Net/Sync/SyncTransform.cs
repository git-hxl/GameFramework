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
        public int SyncFrames = 10;
        public int CacheCount = 5;

        public bool IsClient { get; private set; }

        private Queue<TransformSnapshoot> transformSnapshoots = new Queue<TransformSnapshoot>();

        private TransformSnapshoot lastSnapshoot;
        private TransformSnapshoot curSnapshoot;

        private float sendTimer;
        private float syncTimer;

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

            if (transformSnapshoots.Count > CacheCount * 2)
            {
                Debug.LogWarning("TransformSnapshoots 缓存警告：" + transformSnapshoots.Count);

                ReacToSnapshoot(curSnapshoot);
            }

            if (curSnapshoot != null)
            {
                long offset = timestamp - curSnapshoot.Timestamp;

                if (offset > 1000)
                {
                    Debug.LogWarning("TransformSnapshoots 同步延迟警告：" + offset);

                    ReacToSnapshoot(curSnapshoot);
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
            if (sendTimer < 1f / SyncFrames)
            {
                return;
            }
            sendTimer = 0;

            SyncTransformData syncTransformData = new SyncTransformData();
            syncTransformData.Position = transform.position;
            if (SyncRotation)
                syncTransformData.Direction = transform.forward;
            if (SyncScale)
                syncTransformData.Scale = transform.localScale;
            byte[] data = MessagePackSerializer.Serialize(syncTransformData);

            NetManager.Instance.SendSyncEvent(SyncCode.SyncTransform, data, LiteNetLib.DeliveryMethod.Sequenced);
        }

        private void SyncTransformData()
        {
            if (transformSnapshoots.Count <= CacheCount)
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

            float delta = syncTimer / (1f / SyncFrames);

            transform.position = Vector3.Lerp(lastSnapshoot.TransformData.Position, curSnapshoot.TransformData.Position, delta);

            if (SyncRotation)
                transform.forward = Vector3.Lerp(lastSnapshoot.TransformData.Direction, curSnapshoot.TransformData.Direction, delta);
            if (SyncScale)
                transform.localScale = Vector3.Lerp(lastSnapshoot.TransformData.Scale, curSnapshoot.TransformData.Scale, delta);

            Debug.DrawLine(transform.position, curSnapshoot.TransformData.Position, Color.red);

            Debug.DrawRay(transform.position, curSnapshoot.TransformData.Direction * 5, Color.blue);

            if (delta >= 1f)
            {
                ReacToSnapshoot(curSnapshoot);
            }
        }

        private void ReacToSnapshoot(TransformSnapshoot snapshoot)
        {
            if (lastSnapshoot != null && lastSnapshoot != snapshoot)
            {
                ReferencePool.Instance.Release(lastSnapshoot);
            }

            lastSnapshoot = snapshoot;

            syncTimer = 0;
        }
    }
}