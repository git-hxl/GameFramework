using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework
{
    public class ObjectPoolExample : MonoBehaviour
    {
        ObjectPoolCollection objectPoolCollection;

        Queue<GameObject> queue = new Queue<GameObject>();
        // Start is called before the first frame update
        void Start()
        {
            objectPoolCollection = ObjectPoolManager.Instance.CreateReferenceCollection( "Assets/GameFramework/Example/ObjectPool/Cube.prefab");
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.F1))
            {
                GameObject gameObject = ObjectPoolManager.Instance.Acquire("Assets/GameFramework/Example/ObjectPool/Cube.prefab");
                gameObject.SetActive(true);
                gameObject.transform.position = new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));

                queue.Enqueue(gameObject);
            }

            if (Input.GetKeyUp(KeyCode.F2))
            {
                if (queue.Count > 0)
                {
                    if (Random.value > 0.5f)
                    {
                        objectPoolCollection.Release(queue.Dequeue());
                    }
                    else
                    {
                        ObjectPoolManager.Instance.Release("Assets/GameFramework/Example/ObjectPool/Cube.prefab",queue.Dequeue());
                    }

                }

            }

            if (Input.GetKeyUp(KeyCode.F3))
            {
                objectPoolCollection.RemoveAll();
            }
        }
    }
}
