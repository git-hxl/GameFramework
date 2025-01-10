
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
        public event Action<PlayerInfo> OnUpdatePlayerInfoEvent;

        public event Action<SyncRequestData> OnSyncRequestEvent;
    }
}
