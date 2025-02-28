
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

        public GameObject Acquire(string assetPath)
        {
            return GetReferenceCollection(assetPath).Acquire();
        }

        public void Release(string assetPath, GameObject reference)
        {
            GetReferenceCollection(assetPath).Release(reference);
        }

        public void Add(string assetPath, int count)
        {
            GetReferenceCollection(assetPath).Add(count);
        }

        public void Remove(string assetPath, int count)
        {
            GetReferenceCollection(assetPath).Remove(count);
        }

        public void RemoveAll(string assetPath)
        {
            GetReferenceCollection(assetPath).RemoveAll();
        }

        private ObjectPoolCollection GetReferenceCollection(string assetPath)
        {
            ObjectPoolCollection referenceCollection = null;
            referenceCollections.TryGetValue(assetPath, out referenceCollection);

            if (referenceCollection == null)
            {
                return CreateReferenceCollection(assetPath);
            }

            return referenceCollection;
        }

        public ObjectPoolCollection CreateReferenceCollection(string assetPath)
        {
            if (referenceCollections.ContainsKey(assetPath))
            {
                Debug.LogError("对象池已存在：" + assetPath);

                return null;
            }

            ObjectPoolCollection referenceCollection = new ObjectPoolCollection(assetPath);
            referenceCollections.Add(assetPath, referenceCollection);

            return referenceCollection;
        }

    }
}