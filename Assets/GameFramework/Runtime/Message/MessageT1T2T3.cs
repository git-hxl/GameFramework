
using System.Collections.Generic;
using System;

namespace GameFramework
{
    public partial class MessageManager
    {
        public void Register<T1, T2, T3>(int msgID, Action<T1, T2, T3> action)
        {
            if (!keyValuePairs.ContainsKey(msgID))
            {
                keyValuePairs[msgID] = new List<Delegate>();
            }
            keyValuePairs[msgID].Add(action);
        }

        public void Dispatch<T1, T2, T3>(int msgID, T1 param1, T2 param2, T3 param3)
        {
            if (keyValuePairs.ContainsKey(msgID))
            {
                for (int i = 0; i < keyValuePairs[msgID].Count; i++)
                {
                    Delegate @delegate = keyValuePairs[msgID][i];
                    if (@delegate is Action<T1, T2, T3>)
                        (@delegate as Action<T1, T2, T3>).Invoke(param1, param2, param3);
                }
            }
        }

        public void UnRegister<T1, T2, T3>(int msgID, Action<T1, T2, T3> action)
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