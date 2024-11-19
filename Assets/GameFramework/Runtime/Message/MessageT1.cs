
using System.Collections.Generic;
using System;

namespace GameFramework
{
    public partial class MessageManager
    {
        public void Register<T>(int msgID, Action<T> action)
        {
            if (!keyValuePairs.ContainsKey(msgID))
            {
                keyValuePairs[msgID] = new List<Delegate>();
            }
            keyValuePairs[msgID].Add(action);
        }

        public void Dispatch<T>(int msgID, T param)
        {
            if (keyValuePairs.ContainsKey(msgID))
            {
                for (int i = 0; i < keyValuePairs[msgID].Count; i++)
                {
                    Delegate @delegate = keyValuePairs[msgID][i];
                    if (@delegate is Action<T>)
                        (@delegate as Action<T>).Invoke(param);
                }
            }
        }

        public void UnRegister<T>(int msgID, Action<T> action)
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