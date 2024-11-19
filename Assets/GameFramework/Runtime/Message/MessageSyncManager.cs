using System;
using System.Collections.Generic;

namespace GameFramework
{
    public class MessageSyncParam : IReference
    {
        public int MsgID;
        public byte[] MsgData;

        public void OnAcquire()
        {

        }

        public void OnRelease()
        {
            MsgID = -1;
            MsgData = null;
        }
    }

    public class MessageSyncManager : MonoSingleton<MessageSyncManager>
    {
        private Dictionary<int, List<Delegate>> keyValuePairs = new Dictionary<int, List<Delegate>>();

        private Queue<MessageSyncParam> messageSyncParams = new Queue<MessageSyncParam>();

        private static object locker = new object();
        protected override void OnDispose()
        {
            keyValuePairs.Clear();
        }

        protected override void OnInit()
        {
            //throw new System.NotImplementedException();
        }

        public void Register(int msgID, Action<byte[]> action)
        {
            if (!keyValuePairs.ContainsKey(msgID))
            {
                keyValuePairs[msgID] = new List<Delegate>();
            }
            keyValuePairs[msgID].Add(action);
        }

        private void Dispatch(MessageSyncParam messageSyncParam)
        {
            int msgID = messageSyncParam.MsgID;
            if (keyValuePairs.ContainsKey(msgID))
            {
                for (int i = 0; i < keyValuePairs[msgID].Count; i++)
                {
                    Delegate @delegate = keyValuePairs[msgID][i];
                    if (@delegate is Action<byte[]>)
                        (@delegate as Action<byte[]>).Invoke(messageSyncParam.MsgData);
                }
            }

            ReferencePool.Instance.Release(messageSyncParam);
        }

        public void UnRegister(int msgID, Action<byte[]> action)
        {
            if (keyValuePairs.ContainsKey(msgID))
            {
                for (int i = 0; i < keyValuePairs[msgID].Count; i++)
                {
                    if (keyValuePairs[msgID][i].Equals(action))
                    {
                        keyValuePairs[msgID].Remove(action);
                        break;
                    }
                }
            }
        }

        public void Enqueue(int msgID, byte[] data)
        {
            lock (locker)
            {
                MessageSyncParam messageSyncParam = ReferencePool.Instance.Acquire<MessageSyncParam>();

                messageSyncParam.MsgID = msgID;
                messageSyncParam.MsgData = data;

                messageSyncParams.Enqueue(messageSyncParam);
            }
        }

        private void Update()
        {
            lock (locker)
            {
                if (messageSyncParams.Count > 0)
                {
                    MessageSyncParam messageSyncParam = messageSyncParams.Dequeue();

                    Dispatch(messageSyncParam);
                }
            }
        }
    }
}
