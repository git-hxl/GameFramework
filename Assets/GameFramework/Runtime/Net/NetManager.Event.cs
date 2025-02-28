
using GameServer;
using GameServer.Protocol;
using System;

namespace GameFramework
{
    public partial class NetManager
    {

        public event Action OnConnectEvent;
        public event Action OnDisconnectEvent;

        public event Action<JoinRoomResponse> OnJoinRoomEvent;

        public event Action<LeaveRoomResponse> OnLeaveRoomEvent;

        public event Action<RoomInfo> OnUpdateRoomInfoEvent;
        public event Action<UserInfo> OnUpdatePlayerInfoEvent;

        public event Action<SyncRequest> OnSyncRequestEvent;

        public event Action<SyncRequest, ObjectData> OnSyncSpawnObjectEvent;
        public event Action<SyncRequest, ObjectData> OnSyncRemoveObjectEvent;
        public event Action<SyncRequest, TransformData> OnSyncTransformEvent;
        public event Action<SyncRequest, AnimationData> OnSyncAnimationEvent;

    }
}
