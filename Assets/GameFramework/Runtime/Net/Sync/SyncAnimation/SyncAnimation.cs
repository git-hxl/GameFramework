using GameServer.Protocol;
using GameServer;
using System;
using System.Collections.Generic;
using UnityEngine;
using MessagePack;

namespace GameFramework
{
    [RequireComponent(typeof(NetComponent), typeof(Animator))]
    public class SyncAnimation : MonoBehaviour
    {
        public bool FollowTransform;
        [Range(1, 30)]
        public int SyncFrames = 15;

        private Queue<AnimationSnapshot> snapShots = new Queue<AnimationSnapshot>();

        private AnimationSnapshot lastSyncSnapshot;

        private SyncAnimationData[] lastSendDatas;

        private float sendTimer;
        private NetComponent netComponent;

        private Animator animator;

        private SyncTransform syncTransform;

        private void Start()
        {
            netComponent = GetComponent<NetComponent>();
            animator = GetComponent<Animator>();

            if (animator != null)
                lastSendDatas = new SyncAnimationData[animator.layerCount];

            if (FollowTransform)
            {
                syncTransform = GetComponent<SyncTransform>();
            }
        }

        public void AddSnapshot(long timestamp, SyncAnimationData data)
        {
            AnimationSnapshot snapshot = ReferencePool.Instance.Acquire<AnimationSnapshot>();
            snapshot.Timestamp = timestamp;
            snapshot.AnimationData = data;

            snapShots.Enqueue(snapshot);

            if (snapShots.Count > 10)
            {
                Debug.LogWarning("AnimationSnapshot 缓存警告：" + snapShots.Count);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (netComponent == null)
                return;
            if (animator == null)
                return;

            if (netComponent.IsLocal)
            {
                SendAnimationData();
            }
            else
            {
                SyncAnimationData();
            }
        }

        private void SendAnimationData()
        {
            sendTimer += Time.unscaledDeltaTime;
            if (sendTimer < 1f / SyncFrames)
            {
                return;
            }
            sendTimer = 0;

            for (int i = 0; i < animator.layerCount; i++)
            {
                int stateHash;
                float normalizedTime;
                if (!CheckAnimStateChanged(out stateHash, out normalizedTime, i))
                {
                    continue;
                }


                Debug.Log("Send播放动画：" + stateHash + " time:" + normalizedTime + " speed： " + animator.speed);

                SyncAnimationData syncAnimationData = lastSendDatas[i];

                syncAnimationData.ObjectID = netComponent.ObjectID;

                syncAnimationData.LayerID = i;
                syncAnimationData.Weight = animator.GetLayerWeight(i);
                syncAnimationData.StateHash = stateHash;
                syncAnimationData.NormalizedTimeTime = normalizedTime;
                syncAnimationData.Speed = animator.speed;

                syncAnimationData.IntParams = new Dictionary<int, int>();
                syncAnimationData.FloatParams = new Dictionary<int, float>();
                syncAnimationData.BoolParams = new Dictionary<int, bool>();

                for (global::System.Int32 j = 0; j < animator.parameterCount; j++)
                {
                    AnimatorControllerParameter param = animator.GetParameter(j);

                    if (param.type == AnimatorControllerParameterType.Int)
                    {
                        syncAnimationData.IntParams.Add(param.nameHash, animator.GetInteger(param.name));
                    }

                    if (param.type == AnimatorControllerParameterType.Float)
                    {
                        syncAnimationData.FloatParams.Add(param.nameHash, animator.GetFloat(param.name));
                    }

                    if (param.type == AnimatorControllerParameterType.Bool)
                    {
                        syncAnimationData.BoolParams.Add(param.nameHash, animator.GetBool(param.name));
                    }
                }

                byte[] data = MessagePackSerializer.Serialize(syncAnimationData);

                NetManager.Instance.Server.SendSyncEvent(NetManager.Instance.PlayerID, SyncEventCode.SyncAnimation, data, LiteNetLib.DeliveryMethod.Sequenced);
            }
        }

        private void SyncAnimationData()
        {
            if (snapShots.Count <= 0)
            {
                return;
            }

            var curSnapshot = snapShots.Peek();

            //跟随位置同步
            if (FollowTransform)
            {
                if (syncTransform.Timestamp < curSnapshot.Timestamp)
                    return;
            }

            if (lastSyncSnapshot == null)
            {
                lastSyncSnapshot = curSnapshot;
            }

            animator.SetLayerWeight(curSnapshot.AnimationData.LayerID, curSnapshot.AnimationData.Weight);

            animator.speed = curSnapshot.AnimationData.Speed;

            if (animator.enabled)
            {
                animator.Play(curSnapshot.AnimationData.StateHash, curSnapshot.AnimationData.LayerID, curSnapshot.AnimationData.NormalizedTimeTime);

                Debug.Log("Sync播放动画：" + curSnapshot.AnimationData.StateHash + " time:" + curSnapshot.AnimationData.NormalizedTimeTime + " speed： " + curSnapshot.AnimationData.Speed);
            }

            for (int j = 0; j < animator.parameterCount; j++)
            {
                AnimatorControllerParameter param = animator.GetParameter(j);

                if (param.type == AnimatorControllerParameterType.Int)
                {
                    foreach (var item in curSnapshot.AnimationData.IntParams)
                    {
                        animator.SetInteger(item.Key, item.Value);
                    }
                }

                if (param.type == AnimatorControllerParameterType.Float)
                {
                    foreach (var item in curSnapshot.AnimationData.FloatParams)
                    {
                        animator.SetFloat(item.Key, item.Value);
                    }
                }

                if (param.type == AnimatorControllerParameterType.Bool)
                {
                    foreach (var item in curSnapshot.AnimationData.BoolParams)
                    {
                        animator.SetBool(item.Key, item.Value);
                    }
                }
            }

            if (lastSyncSnapshot != null && lastSyncSnapshot != curSnapshot)
            {
                ReferencePool.Instance.Release(lastSyncSnapshot);
            }

            lastSyncSnapshot = snapShots.Dequeue();
        }

        private bool CheckAnimStateChanged(out int stateHash, out float normalizedTime, int layerId)
        {
            bool change = false;
            stateHash = 0;
            normalizedTime = 0;

            var lastSendData = lastSendDatas[layerId];

            if (lastSendData == null)
            {
                lastSendData = new SyncAnimationData();

                lastSendDatas[layerId] = lastSendData;
            }

            float weight = animator.GetLayerWeight(layerId);

            if (Mathf.Abs(weight - lastSendData.Weight) > 0.001f)
            {
                change = true;
            }

            if (Mathf.Abs(animator.speed - lastSendData.Speed) > 0.001f)
            {
                change = true;
            }

            if (animator.IsInTransition(layerId))
            {
                AnimatorStateInfo animatorStateInfo = animator.GetNextAnimatorStateInfo(layerId);
                stateHash = animatorStateInfo.fullPathHash;
                normalizedTime = 0f;
            }
            else
            {
                AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(layerId);
                stateHash = animatorStateInfo.fullPathHash;
                normalizedTime = animatorStateInfo.normalizedTime;
            }

            if (lastSendData.StateHash != stateHash)
            {
                return true;
            }

            return change;
        }
    }
}
