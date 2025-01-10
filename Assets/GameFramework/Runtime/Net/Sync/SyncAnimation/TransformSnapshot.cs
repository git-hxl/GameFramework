using GameFramework;
using GameServer.Protocol;

namespace GameServer
{
    public class AnimationSnapshot : IReference
    {
        public long Timestamp;
        public SyncAnimationData AnimationData;
        public void OnAcquire()
        {

        }

        public void OnRelease()
        {
            Timestamp = -1;
            AnimationData = null;
        }
    }
}
