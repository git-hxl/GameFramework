
namespace GameFramework
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private bool isInited = false;
        private static T instance;
        private static object locker = new object();
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                            instance.Init();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 初始化单例
        /// </summary>
        public virtual void Init()
        {
            if (isInited)
                return;

            isInited = true;
            OnInit();
        }

        /// <summary>
        /// 释放单例
        /// </summary>
        public void Dispose()
        {
            instance = null;
            isInited = false;
            OnDispose();
        }

        protected abstract void OnInit();
        protected abstract void OnDispose();

    }
}