using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ObjectPoolCollection
    {
        private Queue<GameObject> references = new Queue<GameObject>();

        private string assetPath;

        public int CurUsingRefCount { get; private set; }

        public int ReferencesCount { get { return references.Count; } }

        public ObjectPoolCollection(string assetPath)
        {
            this.assetPath = assetPath;
            CurUsingRefCount = 0;
        }

        public GameObject Acquire()
        {
            CurUsingRefCount++;

            GameObject gameObject = null;

            if (references.Count > 0)
            {
                gameObject = references.Dequeue();
            }

            if (gameObject == null)
            {
                var asset = ResourceManager.Instance.LoadAsset<GameObject>(assetPath);

                gameObject = GameObject.Instantiate(asset);
            }

            gameObject.SetActive(true);

            return gameObject;
        }

        public void Release(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            gameObject.SetActive(false);

            if (references.Contains(gameObject))
            {
                Debug.LogError($"重复回收:{gameObject.ToString()}");
                return;
            }
            references.Enqueue(gameObject);

            CurUsingRefCount--;
        }

        public void Add(int count)
        {
            var asset = ResourceManager.Instance.LoadAsset<GameObject>(assetPath);

            while (count-- > 0)
            {
                GameObject gameObject = GameObject.Instantiate(asset);

                gameObject.SetActive(false);
                references.Enqueue(gameObject);
            }
        }

        public void Remove(int count)
        {

            if (count > references.Count)
            {
                count = references.Count;
            }
            while (count-- > 0)
            {
                GameObject item = references.Dequeue();
                UnityEngine.GameObject.Destroy(item);
            }
        }

        public void RemoveAll()
        {
            foreach (var item in references)
            {
                UnityEngine.GameObject.Destroy(item);
            }

            references.Clear();
        }
    }
}
