using GameServer.Protocol;
using MessagePack;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class SyncTransform : MonoBehaviour
    {
        [Range(1,10)]
        public int Buffer = 1;
        public bool SyncPosition = true;
        public bool SyncRotation = true;
        [Range(1, 30)]
        public int SyncInterval = 10;
        private Queue<SyncTransformData> syncTransformDatas = new Queue<SyncTransformData>();
        private float syncTimer;
        private bool isLocal;

        private int id;
        public void Init(int id)
        {
            this.id = id;

            isLocal = NetManager.Instance.ID == id;
        }

        public void OnReceiveRemoteData(SyncTransformData data)
        {
            syncTransformDatas.Enqueue(data);
            if (syncTransformDatas.Count >= 10)
            {
                Debug.LogWarning("缓存数据过多！" + syncTransformDatas.Count);
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if(isLocal)
            {
                syncTimer += Time.deltaTime;
                if (syncTimer > 1f / SyncInterval)
                {
                    SendTransformData();

                    syncTimer = 0;
                }
            }
            else
            {
                SyncTransformData();
            }
        }

        private void SendTransformData()
        {
            SyncTransformData syncTransformData = new SyncTransformData();
            syncTransformData.Position = transform.position;
            syncTransformData.Rotation = transform.rotation;

            byte[] data = MessagePackSerializer.Serialize(syncTransformData);

            NetManager.Instance.SendEvent(GameServer.EventCode.SyncTransform, data, LiteNetLib.DeliveryMethod.Sequenced);
        }

        private void SyncTransformData()
        {
            if (syncTransformDatas.Count > Buffer)
            {
                SyncTransformData syncTransformData = syncTransformDatas.Peek();
                float delta = SmoothSpeed * Time.deltaTime;

                transform.position = Vector3.Lerp(transform.position, syncTransformData.Position, delta);

                float distance = Vector3.Distance(transform.position, syncTransformData.Position);

                if (SyncRotation)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, syncTransformData.Rotation, delta);
                }
                if (distance <= delta)
                {
                    syncTransformDatas.Dequeue();
                }

            }
        }
    }
}
