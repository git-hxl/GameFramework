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
        public float SyncInterval = 0.1f;
        public bool IsFixedUpdate = false;

        private Queue<AnimationData> queueData = new Queue<AnimationData>();
        private AnimationData lastsyncData;
        private Dictionary<int, AnimationData> lastSendDatas = new Dictionary<int, AnimationData>();

        private float lastSendTime;
        private float syncTimer;
        private NetComponent netComponent;
        private Animator animator;

        private void Start()
        {
            netComponent = GetComponent<NetComponent>();
            animator = GetComponent<Animator>();

            if (animator != null)
            {
                for (int i = 0; i < animator.layerCount; i++)
                {
                    lastSendDatas.Add(i, new AnimationData());
                }
            }

        }

        public void EnqueueData(AnimationData data)
        {
            queueData.Enqueue(data);

            if (queueData.Count > 10)
            {
                Debug.LogWarning("queueData 缓存警告：" + queueData.Count);
            }
        }

        private void FixedUpdate()
        {
            if (!IsFixedUpdate)
                return;

            if (netComponent == null)
                return;

            if (netComponent.IsLocal)
            {
                SendAnimationData(Time.fixedUnscaledDeltaTime);
            }
            else
            {
                SyncAnimationData(Time.fixedUnscaledDeltaTime);
            }
        }

        private void Update()
        {
            if (IsFixedUpdate)
                return;

            if (netComponent == null)
                return;

            if (netComponent.IsLocal)
            {
                SendAnimationData(Time.unscaledDeltaTime);
            }
            else
            {
                SyncAnimationData(Time.unscaledDeltaTime);
            }
        }

        private void SendAnimationData(float deltaTime)
        {
            if ((Time.time - lastSendTime) < SyncInterval)
            {
                return;
            }

            lastSendTime = Time.time;

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (CheckAnimStateChanged(i) == false)
                    continue;

                int stateHash;
                float normalizedTime;

                if (animator.IsInTransition(i))
                {
                    AnimatorStateInfo animatorStateInfo = animator.GetNextAnimatorStateInfo(i);
                    stateHash = animatorStateInfo.fullPathHash;
                    normalizedTime = 0f;
                }
                else
                {
                    AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(i);
                    stateHash = animatorStateInfo.fullPathHash;
                    normalizedTime = animatorStateInfo.normalizedTime;
                }

               // Debug.Log("Send播放动画：" + stateHash + " time:" + normalizedTime + " speed： " + animator.speed);

                SyncRequest syncRequest = new SyncRequest();
                syncRequest.SyncCode = SyncCode.SyncAnimation;

                AnimationData animationData = lastSendDatas[i];

                animationData.ObjectID = netComponent.ObjectID;

                animationData.LayerID = i;
                animationData.Weight = animator.GetLayerWeight(i);
                animationData.StateHash = stateHash;
                animationData.NormalizedTimeTime = normalizedTime;
                animationData.Speed = animator.speed;

                animationData.IntParams = new Dictionary<int, int>();
                animationData.FloatParams = new Dictionary<int, float>();
                animationData.BoolParams = new Dictionary<int, bool>();

                for (global::System.Int32 j = 0; j < animator.parameterCount; j++)
                {
                    AnimatorControllerParameter param = animator.GetParameter(j);

                    if (param.type == AnimatorControllerParameterType.Int)
                    {
                        animationData.IntParams.Add(param.nameHash, animator.GetInteger(param.name));
                    }

                    if (param.type == AnimatorControllerParameterType.Float && animator.IsParameterControlledByCurve(param.name) == false)
                    {
                        animationData.FloatParams.Add(param.nameHash, animator.GetFloat(param.name));
                    }

                    if (param.type == AnimatorControllerParameterType.Bool)
                    {
                        animationData.BoolParams.Add(param.nameHash, animator.GetBool(param.name));
                    }
                }

                syncRequest.SyncData = MessagePackSerializer.Serialize(animationData);

                NetManager.Instance.SendRequest(OperationCode.SyncEvent, syncRequest, LiteNetLib.DeliveryMethod.Sequenced);
            }
        }

        private void SyncAnimationData(float deltaTime)
        {
            if (queueData.Count <= 0)
            {
                lastsyncData = null;
                syncTimer = 0;
                return;
            }
            var curSyncData = queueData.Peek();

            if (lastsyncData == null)
            {
                lastsyncData = curSyncData;
            }

            animator.SetLayerWeight(curSyncData.LayerID, curSyncData.Weight);

            animator.speed = curSyncData.Speed;

            if (animator.enabled)
            {
                animator.Play(curSyncData.StateHash, curSyncData.LayerID, curSyncData.NormalizedTimeTime);

                //Debug.Log("Sync播放动画：" + curSnapshot.AnimationData.StateHash + " time:" + curSnapshot.AnimationData.NormalizedTimeTime + " speed： " + curSnapshot.AnimationData.Speed);
            }

            for (int j = 0; j < animator.parameterCount; j++)
            {
                AnimatorControllerParameter param = animator.GetParameter(j);

                if (param.type == AnimatorControllerParameterType.Int)
                {
                    foreach (var item in curSyncData.IntParams)
                    {
                        animator.SetInteger(item.Key, item.Value);
                    }
                }

                if (param.type == AnimatorControllerParameterType.Float)
                {
                    foreach (var item in curSyncData.FloatParams)
                    {
                        animator.SetFloat(item.Key, item.Value);
                    }
                }

                if (param.type == AnimatorControllerParameterType.Bool)
                {
                    foreach (var item in curSyncData.BoolParams)
                    {
                        animator.SetBool(item.Key, item.Value);
                    }
                }
            }


            lastsyncData = queueData.Dequeue();
            syncTimer = 0f;
        }

        private bool CheckAnimStateChanged(int layerId)
        {
            var lastSendData = lastSendDatas[layerId];

            float weight = animator.GetLayerWeight(layerId);

            if (Mathf.Abs(weight - lastSendData.Weight) > 0.001f)
            {
                return true;
            }

            if (Mathf.Abs(animator.speed - lastSendData.Speed) > 0.001f)
            {
                return true;
            }

            AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(layerId);
            int newStateHash = animatorStateInfo.fullPathHash;

            if (lastSendData.StateHash != newStateHash)
            {
                return true;
            }

            return false;
        }
    }
}
