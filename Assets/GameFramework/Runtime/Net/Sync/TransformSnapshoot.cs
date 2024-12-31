using GameFramework;
using GameServer.Protocol;

namespace GameServer
{
    public class TransformSnapshoot : IReference
    {
        public long Timestamp;
        public SyncTransformData TransformData;
        public void OnAcquire()
        {

        }

        public void OnRelease()
        {
            Timestamp = -1;
            TransformData = null;
        }
    }
}
