
using GameServer.Protocol;

namespace GameFramework
{
    public interface INetEvent
    {
        void OnConnect();
        void OnDisconnect();

        void OnJoinRoom(JoinRoomResponse joinRoomResponse);
        void OnOtherJoinRoom(PlayerInfo playerInfo);

        void OnLeaveRoom();
        void OnOtherLeaveRoom(PlayerInfo playerInfo);
    }
}
