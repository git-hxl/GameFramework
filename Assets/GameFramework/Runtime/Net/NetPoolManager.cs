
using GameServer.Protocol;
using GameServer;
using MessagePack;
using UnityEngine;
using System.Collections.Generic;
using System;


namespace GameFramework
{
    public class NetPoolManager : Singleton<NetPoolManager>
    {
        private Dictionary<int, Dictionary<int, NetComponent>> netObjects = new Dictionary<int, Dictionary<int, NetComponent>>();
        protected override void OnInit()
        {
            //throw new System.NotImplementedException();
            NetManager.Instance.OnSyncRequestEvent += Instance_OnSyncRequestEvent;
        }

        protected override void OnDispose()
        {
            // throw new System.NotImplementedException();

            netObjects.Clear();
        }

        public NetComponent SpawnLocalObject(string poolName)
        {
            GameObject gameObject = ObjectPoolManager.Instance.Acquire(poolName);
            NetComponent netComponent = gameObject.GetComponent<NetComponent>();
            if (netComponent == null)
            {
                netComponent = gameObject.AddComponent<NetComponent>();
            }

            int objectID = gameObject.GetInstanceID();

            netComponent.Init(NetManager.Instance.PlayerID, objectID, true);

            gameObject.name = NetManager.Instance.PlayerID.ToString();
            SyncObjectData syncObjectData = new SyncObjectData();
            syncObjectData.PlayerID = NetManager.Instance.PlayerID;
            syncObjectData.ObjectID = objectID;
            syncObjectData.PoolName = poolName + "_Remote";
            syncObjectData.Active = true;

            byte[] data = MessagePackSerializer.Serialize(syncObjectData);

            NetManager.Instance.Server.SendSyncEvent(NetManager.Instance.PlayerID, SyncEventCode.SyncObject, data, LiteNetLib.DeliveryMethod.ReliableOrdered);

            return netComponent;
        }

        public void SpawnRemoteObject(SyncObjectData syncObjectData)
        {
            string poolName = syncObjectData.PoolName;
            GameObject gameObject = ObjectPoolManager.Instance.Acquire(poolName);
            NetComponent netComponent = gameObject.GetComponent<NetComponent>();
            if (netComponent == null)
            {
                netComponent = gameObject.AddComponent<NetComponent>();
            }

            netComponent.Init(syncObjectData.PlayerID, syncObjectData.ObjectID, false);

            if (!netObjects.ContainsKey(syncObjectData.PlayerID))
            {
                netObjects.Add(syncObjectData.PlayerID, new Dictionary<int, NetComponent>());
            }

            netObjects[syncObjectData.PlayerID][netComponent.ObjectID] = netComponent;

            netComponent.gameObject.name = syncObjectData.PlayerID + "_Remote";
        }

        public void RemoveLocalObject(NetComponent netComponent)
        {
            if (netComponent.IsLocal)
            {
                PoolComponent poolComponent = netComponent.GetComponent<PoolComponent>();
                SyncObjectData syncObjectData = new SyncObjectData();
                syncObjectData.PlayerID = NetManager.Instance.PlayerID;
                syncObjectData.ObjectID = netComponent.ObjectID;
                syncObjectData.PoolName = poolComponent.PoolName;
                syncObjectData.Active = false;

                byte[] data = MessagePackSerializer.Serialize(syncObjectData);

                NetManager.Instance.Server.SendSyncEvent(NetManager.Instance.PlayerID, SyncEventCode.SyncObject, data, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }

            ObjectPoolManager.Instance.Release(netComponent.gameObject);
        }

        public void RemoveRemoteObject(SyncObjectData syncObjectData)
        {
            var netComponent = GetObject(syncObjectData.PlayerID, syncObjectData.ObjectID);

            if (netComponent == null)
                return;

            if (netObjects.ContainsKey(netComponent.PlayerID))
            {
                if (netObjects[netComponent.PlayerID].ContainsKey(netComponent.ObjectID))
                {
                    netObjects[netComponent.PlayerID].Remove(netComponent.ObjectID);
                }
            }

            ObjectPoolManager.Instance.Release(netComponent.gameObject);
        }


        public NetComponent GetObject(int playerID, int objectID)
        {
            if (netObjects.ContainsKey(playerID))
            {
                if (netObjects[playerID].ContainsKey(objectID))
                { return netObjects[playerID][objectID]; }
            }
            return null;
        }

        private void Instance_OnSyncRequestEvent(long timestamp, SyncEventRequest syncRequestData)
        {
            NetComponent netComponent = null;
            switch (syncRequestData.SyncEventCode)
            {
                case SyncEventCode.SyncObject:

                    SyncObjectData syncObjectData = MessagePackSerializer.Deserialize<SyncObjectData>(syncRequestData.SyncData);

                    if (syncObjectData.Active)
                    {
                        SpawnRemoteObject(syncObjectData);
                    }
                    else
                    {
                        RemoveRemoteObject(syncObjectData);
                    }
                    break;

                case SyncEventCode.SyncTransform:
                    SyncTransformData syncTransformData = MessagePackSerializer.Deserialize<SyncTransformData>(syncRequestData.SyncData);

                    netComponent = GetObject(syncRequestData.PlayerID, syncTransformData.ObjectID);

                    if (netComponent == null)
                        return;

                    SyncTransform syncTransform = netComponent.GetComponent<SyncTransform>();

                    syncTransform.AddSnapshot(timestamp, syncTransformData);

                    break;

                case SyncEventCode.SyncAnimation:

                    SyncAnimationData syncAnimationData = MessagePackSerializer.Deserialize<SyncAnimationData>(syncRequestData.SyncData);

                    netComponent = GetObject(syncRequestData.PlayerID, syncAnimationData.ObjectID);

                    if (netComponent == null)
                        return;

                    SyncAnimation syncAnimation = netComponent.GetComponent<SyncAnimation>();

                    syncAnimation.AddSnapshot(timestamp, syncAnimationData);
                    break;
            }
        }
    }

}