
using MessagePack;
using UnityEngine;
using System.Collections.Generic;
using GameServer.Protocol;
using GameServer;
using System;


namespace GameFramework
{
    public class NetPoolManager : Singleton<NetPoolManager>
    {
        private Dictionary<int, GameObject> spawnObjects = new Dictionary<int, GameObject>();

        protected override void OnInit()
        {
            NetManager.Instance.OnSyncSpawnObjectEvent += Instance_OnSyncSpawnObjectEvent;

            NetManager.Instance.OnSyncRemoveObjectEvent += Instance_OnSyncRemoveObjectEvent;
        }

        private void Instance_OnSyncRemoveObjectEvent(SyncRequest arg1, ObjectData arg2)
        {
            RemoveObject(arg2.AssetPath, arg2.ObjectID);
        }

        private void Instance_OnSyncSpawnObjectEvent(SyncRequest arg1, ObjectData arg2)
        {
            if (arg1.UserID == NetManager.Instance.UserID)
            {
                return;
            }

            SpawnObject(arg2.AssetPath, arg1.UserID, arg2.ObjectID, false);
        }

        protected override void OnDispose()
        {
            NetManager.Instance.OnSyncSpawnObjectEvent -= Instance_OnSyncSpawnObjectEvent;

            NetManager.Instance.OnSyncRemoveObjectEvent -= Instance_OnSyncRemoveObjectEvent;
        }

        /// <summary>
        /// 生成网络对象
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="playerID"></param>
        /// <param name="objectID"></param>
        /// <param name="isLocal"></param>
        /// <returns></returns>
        public NetComponent SpawnObject(string assetPath, int playerID, int objectID = -1, bool isLocal = false)
        {
            if (spawnObjects.ContainsKey(objectID))
            {
                Debug.LogError("网络对象添加异常，id已存在！！！");
                //return null;
            }

            ObjectPool objectPool = ObjectPoolManager.Instance.GetPool(assetPath);

            GameObject gameObject = objectPool.Spawn();
            NetComponent netComponent = gameObject.GetComponent<NetComponent>();
            if (netComponent == null)
            {
                netComponent = gameObject.AddComponent<NetComponent>();
            }
            if (objectID == -1)
                objectID = gameObject.GetInstanceID();
            if (isLocal)
            {
                ObjectData objectData = new ObjectData();

                objectData.ObjectID = objectID;
                objectData.AssetPath = assetPath;

                SyncRequest syncRequest = new SyncRequest();

                syncRequest.SyncCode = GameServer.SyncCode.SyncSpawnObject;

                syncRequest.SyncData = MessagePackSerializer.Serialize(objectData);

                NetManager.Instance.SendRequest(OperationCode.SyncEvent, syncRequest, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }

            if (!spawnObjects.ContainsKey(objectID))
            {
                spawnObjects.Add(objectID, gameObject);
            }
            netComponent.Init(playerID, objectID, isLocal);
            return netComponent;
        }

        /// <summary>
        /// 移除网络对象
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="gameObject"></param>
        public void RemoveObject(string assetPath, GameObject gameObject)
        {
            if (gameObject == null)
                return;

            NetComponent netComponent = gameObject.GetComponent<NetComponent>();
            if (netComponent == null)
            {
                Debug.LogError("NetComponent 不存在！！！");
                return;
            }
            if (netComponent.IsLocal)
            {
                ObjectData objectData = new ObjectData();

                objectData.ObjectID = netComponent.ObjectID;
                objectData.AssetPath = assetPath;

                SyncRequest syncRequest = new SyncRequest();

                syncRequest.SyncCode = GameServer.SyncCode.SyncRemoveObject;

                syncRequest.SyncData = MessagePackSerializer.Serialize(objectData);

                NetManager.Instance.SendRequest(OperationCode.SyncEvent, syncRequest, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }

            if (spawnObjects.ContainsKey(netComponent.ObjectID))
            {
                spawnObjects.Remove(netComponent.ObjectID);
            }

            ObjectPoolManager.Instance.GetPool(assetPath).Despawn(gameObject);
        }

        public void RemoveObject(string assetPath, int objectID)
        {
            if (!spawnObjects.ContainsKey(objectID))
            {
                Debug.LogError("网络对象移除异常，id不存在！！！");
                return;
            }
            GameObject gameObject = spawnObjects[objectID];

            RemoveObject(assetPath, gameObject);
        }
    }

}