using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public interface IPoolObject
    {
        void OnSpawn();
        void OnDespawn();
    }
}
