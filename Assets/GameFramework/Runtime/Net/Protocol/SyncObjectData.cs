

using MessagePack;

namespace GameServer.Protocol
{
    [MessagePackObject(true)]
    public class SyncObjectData
    {
        public int PlayerID { get; set; }

        public string PoolName { get; set; }

        public int ObjectID { get; set; }

        public bool Active { get; set; }
    }
}
