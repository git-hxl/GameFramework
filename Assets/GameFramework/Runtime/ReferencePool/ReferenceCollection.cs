using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ReferenceCollection
    {
        private Queue<IReference> references = new Queue<IReference>();

        private Type referenceType;

        public int CurUsingRefCount { get; private set; }
        public int AcquireRefCount { get; private set; }
        public int ReleaseRefCount { get; private set; }
        public int AddRefCount { get; private set; }
        public int RemoveRefCount { get; private set; }


        private static object locker = new object();

        public ReferenceCollection(Type type)
        {
            referenceType = type;

            CurUsingRefCount = 0;
            AcquireRefCount = 0;
            ReleaseRefCount = 0;
            AddRefCount = 0;
            RemoveRefCount = 0;
        }

        public T Acquire<T>() where T : IReference, new()
        {
            T t = default(T);

            if (typeof(T) != referenceType)
            {
               Debug.LogError($"请求类型错误:{typeof(T).Name}");

               return default(T);
            }


            CurUsingRefCount++;
            AcquireRefCount++;

            lock (locker)
            {
                if (references.Count > 0)
                {
                    t = (T)references.Dequeue();
                }

                AddRefCount++;

                t = new T();
                t.OnAcquire();

                return t;
            }
        }

        public void Release(IReference reference)
        {
            reference.OnRelease();
            lock (locker)
            {
                if (references.Contains(reference))
                {
                    Debug.LogError($"重复回收:{reference.ToString()}");
                    return;
                }
                references.Enqueue(reference);
            }
            CurUsingRefCount--;
            ReleaseRefCount++;
        }

        public void Add<T>(int count) where T : IReference, new()
        {
            if (typeof(T) != referenceType)
            {
                Debug.LogError($"请求类型错误:{typeof(T).Name}");

                return;
            }
            lock (locker)
            {
                AddRefCount += count;
                while (count-- > 0)
                {
                    references.Enqueue(new T());
                }
            }
        }

        public void Remove(int count)
        {
            lock (locker)
            {
                if (count > references.Count)
                {
                    count = references.Count;
                }

                RemoveRefCount += count;

                while (count-- > 0)
                {
                    references.Dequeue();
                }
            }
        }

        public void RemoveAll()
        {
            lock (locker)
            {
                RemoveRefCount += references.Count;
                references.Clear();
            }
        }
    }
}
