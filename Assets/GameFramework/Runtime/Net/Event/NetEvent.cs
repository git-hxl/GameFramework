

using GameServer.Protocol;
using System;

namespace GameFramework
{
    public class NetEvent
    {
        public static Action OnConnect;
        public static Action OnDisconnect;
        public static Action<JoinRoomResponse> OnJoinRoom;
        public static Action<PlayerInfoInRoom> OnOtherJoinRoom;
        public static Action OnLeaveRoom;
        public static Action<PlayerInfoInRoom> OnOtherLeaveRoom;
        public static Action<SyncEventData> OnSyncEvent;

        public static void Register(INetEvent netEvent)
        {
            OnConnect += netEvent.OnConnect;
            OnDisconnect += netEvent.OnDisconnect;
            OnJoinRoom += netEvent.OnJoinRoom;
            OnOtherJoinRoom += netEvent.OnOtherJoinRoom;
            OnLeaveRoom += netEvent.OnLeaveRoom;
            OnOtherLeaveRoom += netEvent.OnOtherLeaveRoom;
            OnSyncEvent += netEvent.OnSyncEvent;
        }

        public static void UnRegister(INetEvent netEvent)
        {
            OnConnect -= netEvent.OnConnect;
            OnDisconnect -= netEvent.OnDisconnect;
            OnJoinRoom -= netEvent.OnJoinRoom;
            OnOtherJoinRoom -= netEvent.OnOtherJoinRoom;
            OnLeaveRoom -= netEvent.OnLeaveRoom;
            OnOtherLeaveRoom -= netEvent.OnOtherLeaveRoom;
            OnSyncEvent -= netEvent.OnSyncEvent;
        }
    }
}