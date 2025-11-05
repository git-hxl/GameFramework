using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ObjectPool
    {
        private Queue<GameObject> poolObjects = new Queue<GameObject>();

        private string assetPath;
        private string poolName;
        private int maxCount;

        private GameObject prefab;

        public ObjectPool(string assetPath, string poolName, int maxCount)
        {
            this.assetPath = assetPath;

            this.poolName = poolName;

            this.maxCount = maxCount;

            prefab = LoadAsset(assetPath);
        }

        private GameObject LoadAsset(string assetPath)
        {
            var asset = ResourceManager.Instance.LoadAsset<GameObject>(assetPath);

            return asset;
        }


        public GameObject Spawn(bool isActive = true)
        {
            if (poolObjects.TryDequeue(out GameObject obj))
            {

            }

            if (obj == null)
            {
                obj = GameObject.Instantiate(prefab);
            }

            obj.name = poolName;

            obj.SetActive(isActive);

            IPoolObject[] objComponents = obj.GetComponents<IPoolObject>();

            foreach (var item in objComponents)
            {
                item.OnSpawn();
            }

            return obj;
        }

        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            if (obj.name != poolName)
            {
                Debug.LogError($"Pool Object {obj.name} Despawn error");
                return;
            }

            if (poolObjects.Contains(obj))
            {
                return;
            }

            IPoolObject[] objComponents = obj.GetComponents<IPoolObject>();

            foreach (var item in objComponents)
            {
                item.OnDespawn();
            }

            if (poolObjects.Count < maxCount)
            {
                poolObjects.Enqueue(obj);
                obj.SetActive(false);
            }
            else
            {
                GameObject.Destroy(obj);
            }
        }

        public void Clear()
        {
            poolObjects.Clear();
        }
    }
}
