
using UnityEngine;

namespace GameFramework
{
    public class PoolComponent : MonoBehaviour
    {
        public string PoolName { get; private set; }

        public void Init(string poolName)
        {
            PoolName = poolName;
        }
    }
}
