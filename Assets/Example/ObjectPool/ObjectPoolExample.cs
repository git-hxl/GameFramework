using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ObjectPoolExample : MonoBehaviour
    {

        void Start()
        {
            ObjectPoolManager.Instance.RegisterPool("Assets/Example/ObjectPool/Cube.prefab","Cube");
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                ObjectPoolManager.Instance.GetPool("Cube").Spawn();
            }

        }
    }
}
