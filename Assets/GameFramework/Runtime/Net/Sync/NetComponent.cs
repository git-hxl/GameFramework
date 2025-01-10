

using UnityEngine;

namespace GameFramework
{
    public class NetComponent : MonoBehaviour
    {
        public int PlayerID { get; private set; }
        public int ObjectID { get; private set; }

        public bool IsLocal { get; set; }


        public void Init(int playerID, int objectID, bool isLocal)
        {
            PlayerID = playerID;

            ObjectID = objectID;

            IsLocal = isLocal;
        }
    }
}