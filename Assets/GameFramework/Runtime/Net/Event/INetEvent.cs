
using GameServer.Protocol;

namespace GameFramework
{
    public interface INetEvent
    {
        void OnConnect();
        void OnDisconnect();

        void OnJoinRoom(JoinRoomResponse joinRoomResponse);
        void OnOtherJoinRoom(PlayerInfoInRoom playerInfoInRoom);

        void OnLeaveRoom();
        void OnOtherLeaveRoom(PlayerInfoInRoom playerInfoInRoom);

        void OnSyncEvent(SyncEventRequest syncEventRequest);
    }
}
