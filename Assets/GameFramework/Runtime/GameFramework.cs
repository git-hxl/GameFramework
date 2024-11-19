
using System.Collections.Generic;


namespace GameFramework
{
    public class GameFramework : MonoSingleton<GameFramework>
    {
        private List<ISystem> _systems = new List<ISystem>();

        protected override void OnDispose()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnDestroy();
            }
            _systems.Clear();
        }

        protected override void OnInit()
        {
            //throw new System.NotImplementedException();
        }

        /// <summary>
        /// 注册系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="system"></param>
        public void RegisterSystem<T>(T system) where T : ISystem
        {
            if (_systems.Contains(system)) return;

            _systems.Add(system);

            system.OnInit();
        }

        /// <summary>
        /// 获取系统
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSystem<T>() where T : ISystem
        {
            foreach (ISystem system in _systems)
            {
                if (system.GetType() == typeof(T)) return (T)system;
            }

            return default(T);
        }


        private void Update()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnUpdate();
            }
        }
    }
}
