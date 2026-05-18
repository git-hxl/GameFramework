using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class AutoDespawn : MonoBehaviour, IPoolObject
    {
        public int Time;

        IEnumerator Despawn()
        {
            yield return new WaitForSeconds(Time);
            ObjectPoolManager.Instance.GetPool(gameObject.name).Despawn(gameObject);
        }

        public void OnDespawn()
        {
            
        }

        public void OnSpawn()
        {
            if (Time > 0)
            {
                StartCoroutine(Despawn());
            }
        }
    }
}
