

using UnityEngine;

namespace GameFramework
{
    public class NetComponent : MonoBehaviour
    {
        [SerializeField]
        private int playerID;
        [SerializeField]
        private int objectID;

        [SerializeField]
        private bool isLocal;

        public int PlayerID { get { return playerID; } }
        public int ObjectID { get { return objectID; } }
        public bool IsLocal { get { return isLocal; } }


        private SyncTransform syncTransform;
        private SyncAnimation syncAnimation;
        public void Init(int playerID, int objectID, bool isLocal)
        {
            this.playerID = playerID;
            this.objectID = objectID;
            this.isLocal = isLocal;
        }

        private void OnEnable()
        {
            NetManager.Instance.OnSyncTransformEvent += Instance_OnSyncTransformEvent;

            NetManager.Instance.OnSyncAnimationEvent += Instance_OnSyncAnimationEvent;

            syncTransform = GetComponent<SyncTransform>();
            syncAnimation = GetComponent<SyncAnimation>();
        }

        private void Instance_OnSyncAnimationEvent(GameServer.Protocol.SyncRequest arg1, GameServer.Protocol.AnimationData arg2)
        {
            if (isLocal == false && arg2.ObjectID == this.ObjectID)
            {
                if (syncAnimation != null)
                {
                    syncAnimation.EnqueueData(arg2);
                }
            }
        }

        private void Instance_OnSyncTransformEvent(GameServer.Protocol.SyncRequest arg1, GameServer.Protocol.TransformData arg2)
        {
            if (isLocal == false && arg2.ObjectID == this.ObjectID)
            {
                if (syncTransform != null)
                {
                    syncTransform.EnqueueData(arg2);
                }
            }
        }


        private void OnDisable()
        {
            NetManager.Instance.OnSyncTransformEvent -= Instance_OnSyncTransformEvent;

            NetManager.Instance.OnSyncAnimationEvent -= Instance_OnSyncAnimationEvent;
        }
    }
}