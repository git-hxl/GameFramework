
using System.Collections.Generic;
using System;

namespace GameFramework
{
    public partial class MessageManager
    {
        public void Register<T1, T2>(int msgID, Action<T1, T2> action)
        {
            if (!keyValuePairs.ContainsKey(msgID))
            {
                keyValuePairs[msgID] = new List<Delegate>();
            }
            keyValuePairs[msgID].Add(action);
        }

        public void Dispatch<T1, T2>(int msgID, T1 param1, T2 param2)
        {
            if (keyValuePairs.ContainsKey(msgID))
            {
                for (int i = 0; i < keyValuePairs[msgID].Count; i++)
                {
                    Delegate @delegate = keyValuePairs[msgID][i];
                    if (@delegate is Action<T1, T2>)
                        (@delegate as Action<T1, T2>).Invoke(param1, param2);
                }
            }
        }

        public void UnRegister<T1, T2>(int msgID, Action<T1, T2> action)
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