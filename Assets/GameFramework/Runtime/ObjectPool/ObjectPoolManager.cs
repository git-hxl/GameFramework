
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ObjectPoolManager : MonoSingleton<ObjectPoolManager>
    {
        private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

        protected override void OnInit()
        {

        }


        protected override void OnDispose()
        {
            pools.Clear();
        }

        public void RegisterPool(string assetPath, string poolName, int maxCount = 100)
        {
            if (pools.ContainsKey(poolName))
            {
                return;
            }

            pools.Add(poolName, new ObjectPool(assetPath, poolName, maxCount));
        }

        public ObjectPool GetPool(string poolName)
        {
            if (pools.ContainsKey(poolName))
                return pools[poolName];
            else
                return null;
        }

    }
}