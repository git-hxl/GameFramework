

using GameServer.Protocol;
using System;

namespace GameFramework
{
    public class NetEvent : Singleton<NetEvent>
    {
        public Action OnConnect;
        public Action OnDisconnect;
        public Action<JoinRoomResponse> OnJoinRoom;
        public Action<PlayerInfoInRoom> OnOtherJoinRoom;
        public Action OnLeaveRoom;
        public Action<PlayerInfoInRoom> OnOtherLeaveRoom;
        public Action<SyncEventRequest> OnSyncEvent;

        public void Register(INetEvent netEvent)
        {
            OnConnect += netEvent.OnConnect;
            OnDisconnect += netEvent.OnDisconnect;
            OnJoinRoom += netEvent.OnJoinRoom;
            OnOtherJoinRoom += netEvent.OnOtherJoinRoom;
            OnLeaveRoom += netEvent.OnLeaveRoom;
            OnOtherLeaveRoom += netEvent.OnOtherLeaveRoom;
            OnSyncEvent += netEvent.OnSyncEvent;
        }

        public void UnRegister(INetEvent netEvent)
        {
            OnConnect -= netEvent.OnConnect;
            OnDisconnect -= netEvent.OnDisconnect;
            OnJoinRoom -= netEvent.OnJoinRoom;
            OnOtherJoinRoom -= netEvent.OnOtherJoinRoom;
            OnLeaveRoom -= netEvent.OnLeaveRoom;
            OnOtherLeaveRoom -= netEvent.OnOtherLeaveRoom;
            OnSyncEvent -= netEvent.OnSyncEvent;
        }

        protected override void OnDispose()
        {
            //throw new NotImplementedException();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();
        }
    }
}