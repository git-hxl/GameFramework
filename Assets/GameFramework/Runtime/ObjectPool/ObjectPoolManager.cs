
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        private Dictionary<string, ObjectPoolCollection> referenceCollections = new Dictionary<string, ObjectPoolCollection>();
        public int Count { get { return referenceCollections.Count; } }

        protected override void OnDispose()
        {
            ClearAll();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();
        }

        public void ClearAll()
        {
            foreach (var item in referenceCollections.Values)
            {
                item.RemoveAll();
            }

            referenceCollections.Clear();
        }

        public GameObject Acquire(string poolName)
        {
            return GetReferenceCollection(poolName).Acquire();
        }

        public void Release(string poolName, GameObject reference)
        {
            GetReferenceCollection(poolName).Release(reference);
        }

        public void Add(string poolName, int count)
        {
            GetReferenceCollection(poolName).Add(count);
        }

        public void Remove(string poolName, int count)
        {
            GetReferenceCollection(poolName).Remove(count);
        }

        public void RemoveAll(string poolName)
        {
            GetReferenceCollection(poolName).RemoveAll();
        }

        private ObjectPoolCollection GetReferenceCollection(string poolName)
        {
            ObjectPoolCollection referenceCollection = null;
            referenceCollections.TryGetValue(poolName, out referenceCollection);
            return referenceCollection;
        }

        public ObjectPoolCollection CreateReferenceCollection(string poolName, string assetPath)
        {
            if (referenceCollections.ContainsKey(poolName))
            {
                Debug.LogError("对象池已存在：" + poolName);

                return null;
            }

            ObjectPoolCollection referenceCollection = new ObjectPoolCollection(poolName, assetPath);
            referenceCollections.Add(poolName, referenceCollection);

            return referenceCollection;
        }

    }
}