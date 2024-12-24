
using MessagePack.Resolvers;
using MessagePack;
using UnityEngine;
using LiteNetLib;
using GameServer.Protocol;
using GameServer;
using LiteNetLib.Utils;

namespace GameFramework
{
    public class NetManager : MonoSingleton<NetManager>
    {
        private LiteNetLib.NetManager netManager;
        private EventBasedNetListener listener;
        protected override void OnDispose()
        {
            //throw new System.NotImplementedException();

            netManager.Stop();
        }

        protected override void OnInit()
        {
            MessagePackInit();

            listener = new EventBasedNetListener();

            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent; ;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent; ;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent; ;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent; ;

            netManager = new LiteNetLib.NetManager(listener);

            netManager.Start();

            netManager.Connect("127.0.0.1", 1111, "qwer123456");
        }

        private void MessagePackInit()
        {
            StaticCompositeResolver.Instance.Register(
                MessagePack.Resolvers.GeneratedResolver.Instance,
                MessagePack.Resolvers.StandardResolver.Instance
           );

            var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);

            MessagePackSerializer.DefaultOptions = option;

            Debug.Log("MessagePack Initialized");
        }

        private void Update()
        {
            if (netManager != null)
            {
                netManager.PollEvents();
            }
        }

        private void OnDestroy()
        {
            if (netManager != null)
                netManager.Stop();
        }

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {


            OperationCode operationCode = (OperationCode)reader.GetUShort();
            ReturnCode returnCode = (ReturnCode)reader.GetUShort();

            Debug.Log(string.Format("Listener_NetworkReceiveEvent {0} {1} {2}", peer.Address.ToString(), operationCode, returnCode));

            switch (operationCode)
            {
                case OperationCode.OnJoinRoom:



                    break;
            }
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log(string.Format("Listener_PeerDisconnectedEvent {0}", peer.Address.ToString()));
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Debug.Log(string.Format("Listener_PeerConnectedEvent {0}", peer.Address.ToString()));

            JoinRoom(88, 888);
            JoinRoom(88, 8888);
            JoinRoom(99, 999);
            JoinRoom(999, 999);
        }

        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Debug.Log(string.Format("Listener_ConnectionRequestEvent {0} {1}", request.RemoteEndPoint.ToString(), request.Data.GetString()));
        }


        public void JoinRoom(int playerID, int roomID)
        {
            JoinRoomRequest joinRoomRequest = new JoinRoomRequest();
            joinRoomRequest.PlayerID = playerID;
            joinRoomRequest.RoomID = roomID;

            byte[] data = MessagePack.MessagePackSerializer.Serialize(joinRoomRequest);

            NetDataWriter netDataWriter = new NetDataWriter();

            netDataWriter.Put((ushort)OperationCode.JoinRoom);

            netDataWriter.Put(data);

            netManager.FirstPeer.Send(netDataWriter, DeliveryMethod.ReliableSequenced);
        }

    }
}