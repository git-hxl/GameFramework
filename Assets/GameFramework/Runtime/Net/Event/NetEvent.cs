

using GameServer;
using GameServer.Protocol;
using MessagePack;
using System;

namespace GameFramework
{
    public class NetEvent
    {
        public event Action OnConnectEvent;
        public event Action OnDisconnectEvent;
        public event Action<JoinRoomResponse> OnJoinRoomEvent;
        public event Action<PlayerInfo> OnOtherJoinRoomEvent;
        public event Action OnLeaveRoomEvent;
        public event Action<PlayerInfo> OnOtherLeaveRoomEvent;

        public event Action<int, long, SyncTransformData> OnSyncTransformEvent;


        public void Register(INetEvent netEvent)
        {
            OnConnectEvent += netEvent.OnConnect;
            OnDisconnectEvent += netEvent.OnDisconnect;
            OnJoinRoomEvent += netEvent.OnJoinRoom;
            OnOtherJoinRoomEvent += netEvent.OnOtherJoinRoom;
            OnLeaveRoomEvent += netEvent.OnLeaveRoom;
            OnOtherLeaveRoomEvent += netEvent.OnOtherLeaveRoom;
        }

        public void Unregister(INetEvent netEvent)
        {
            OnConnectEvent -= netEvent.OnConnect;
            OnDisconnectEvent -= netEvent.OnDisconnect;
            OnJoinRoomEvent -= netEvent.OnJoinRoom;
            OnOtherJoinRoomEvent -= netEvent.OnOtherJoinRoom;
            OnLeaveRoomEvent -= netEvent.OnLeaveRoom;
            OnOtherLeaveRoomEvent -= netEvent.OnOtherLeaveRoom;
        }


        public void OnConnect()
        {
            OnConnectEvent();
        }
        public void OnDisconnect()
        {
            OnDisconnectEvent();
        }

        public void OnJoinRoom(JoinRoomResponse joinRoomResponse)
        {
            OnJoinRoomEvent(joinRoomResponse);
        }

        public void OnOtherJoinRoom(PlayerInfo playerInfo)
        {
            OnOtherJoinRoomEvent(playerInfo);
        }

        public void OnLeaveRoom()
        {
            OnLeaveRoomEvent();
        }

        public void OnOtherLeaveRoom(PlayerInfo playerInfo)
        {
            OnOtherLeaveRoomEvent(playerInfo);
        }


        public void OnSync(SyncRequest syncRequest)
        {
            SyncCode syncCode = (SyncCode)(syncRequest.SyncCode);

            switch (syncCode)
            {
                case SyncCode.SyncTransform:

                    SyncTransformData syncTransformData = MessagePackSerializer.Deserialize<SyncTransformData>(syncRequest.SyncData);

                    OnSyncTransformEvent(syncRequest.PlayerID, syncRequest.Timestamp, syncTransformData);

                    break;
            }
        }
    }
}