using GameServer.Protocol;
using MessagePack;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework
{
    public class SyncTransform : MonoBehaviour
    {
        [Range(1, 10)]
        public int Buffer = 1;
        public bool SyncPosition = true;
        public bool SyncRotation = true;
        [Range(1, 30)]
        public int SyncInterval = 10;

        private Dictionary<long, SyncTransformData> syncTransformDatas = new Dictionary<long, SyncTransformData>();

        //private Queue<SyncTransformData> syncTransformDatas = new Queue<SyncTransformData>();

        private float syncTimer;
        private SyncTransformData lastSendData;

        private bool isLocal;

        private int id;

        private long lastTimeStamp;

        private Vector3 lastPos;
        private Quaternion lastRot;

        private void Start()
        {
            
        }

        public void Init(int id)
        {
            this.id = id;

            isLocal = NetManager.Instance.PlayerID == id;
        }

        public void OnReceiveRemoteData(long timeStamp, SyncTransformData data)
        {

            syncTransformDatas.Add(timeStamp, data);
            if (lastTimeStamp == 0)
            {
                lastTimeStamp = timeStamp;

                transform.position = data.Position;
                transform.rotation = data.Rotation;

                lastPos = data.Position;
                lastRot = data.Rotation;
            }


            Debug.Log("OnReceiveRemoteData:" + Time.time);
            //else
            //{
            //    long offset = timeStamp - lastTimeStamp;

            //    float count = offset / 1000f / Time.fixedDeltaTime;

            //    for (float i = 0; i < count; i++)
            //    {
            //        Vector3 syncPos = Vector3.Lerp(data.Position, data.Position, i / count);

            //        Quaternion syncRotation = Quaternion.Lerp(data.Rotation, data.Rotation, i / count);

            //        data = new SyncTransformData();

            //        data.Position = syncPos;
            //        data.Rotation = syncRotation;

            //        syncTransformDatas.Enqueue(data);
            //    }
            //}
            //lastTimeStamp = timeStamp;
            //lastReceiveData = data;
        }

        // Update is called once per frame
        void Update()
        {
            if (isLocal)
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
            if (lastSendData != null)
            {
                if (Vector3.Distance(lastSendData.Position, transform.position) < 0.001f)
                {
                    return;
                }
            }

            SyncTransformData syncTransformData = new SyncTransformData();
            syncTransformData.Position = transform.position;
            syncTransformData.Rotation = transform.rotation;
            byte[] data = MessagePackSerializer.Serialize(syncTransformData);

            NetManager.Instance.SendEvent(GameServer.EventCode.SyncTransform, data, LiteNetLib.DeliveryMethod.Sequenced);

            lastSendData = syncTransformData;

            Debug.Log("SendTransformData:" + Time.time);
        }

        float syncTime;
        private void SyncTransformData()
        {
            if (syncTransformDatas.Count <= 1 && lastTimeStamp != 0)
            {
                lastTimeStamp += (int)(Time.deltaTime * 1000);
            }

            if (syncTransformDatas.Count >= 2)
            {
                var syncTransformDatas2 = syncTransformDatas.OrderBy((a) => a.Key);

                foreach (var item in syncTransformDatas2)
                {
                    if (lastTimeStamp < item.Key)
                    {
                        long offset = item.Key - lastTimeStamp;

                        syncTime += Time.deltaTime;

                        float delta = syncTime / (offset / 1000f);

                        Debug.Log($"SyncTransformData offset {offset}  delta {delta}");

                        transform.position = Vector3.Lerp(lastPos, item.Value.Position, delta);

                        transform.rotation = Quaternion.Lerp(lastRot, item.Value.Rotation, delta);

                        if (delta >= 1f)
                        {
                            lastRot = transform.rotation;
                            lastPos = transform.position;

                            lastTimeStamp = item.Key;

                            syncTime = 0;

                            syncTransformDatas.Remove(item.Key);
                        }

                        break;
                    }
                }
            }
        }

        public void OnReceiveSync(SyncEventRequest syncEventRequest)
        {


            SyncTransformData syncTransformData = MessagePackSerializer.Deserialize<SyncTransformData>(syncEventRequest.SyncData);
        }
    }
}
