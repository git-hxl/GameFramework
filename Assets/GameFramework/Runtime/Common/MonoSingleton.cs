
using UnityEngine;

namespace GameFramework
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private bool isInited = false;
        private static T instance = null;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError(typeof(T).Name + " is Null, MonoBehaviour need a GameObject!");
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (isInited)
                return;

            isInited = true;

            instance = this as T;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"{typeof(T).Name} {gameObject.name} Inited!");
            OnInit();
        }

        public void Dispose()
        {
            isInited = false;
            instance = null;
            OnDispose();
            Destroy(gameObject);
        }

        protected abstract void OnInit();
        protected abstract void OnDispose();
    }
}
