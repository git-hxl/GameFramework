

using System.Collections.Generic;
using System;

namespace GameFramework
{
    public partial class MessageManager : Singleton<MessageManager>
    {
        private Dictionary<int, List<Delegate>> keyValuePairs = new Dictionary<int, List<Delegate>>();

        protected override void OnDispose()
        {
            keyValuePairs.Clear();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();
        }

        public void Register(int msgID, Action action)
        {
            if (!keyValuePairs.ContainsKey(msgID))
            {
                keyValuePairs[msgID] = new List<Delegate>();
            }
            keyValuePairs[msgID].Add(action);
        }

        public void Dispatch(int msgID)
        {
            if (keyValuePairs.ContainsKey(msgID))
            {
                for (int i = 0; i < keyValuePairs[msgID].Count; i++)
                {
                    Delegate @delegate = keyValuePairs[msgID][i];
                    if (@delegate is Action)
                        (@delegate as Action).Invoke();
                }
            }
        }

        public void UnRegister(int msgID, Action action)
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
    }


}
