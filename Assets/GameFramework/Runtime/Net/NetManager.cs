
using MessagePack.Resolvers;
using MessagePack;
using UnityEngine;
using LiteNetLib;
using GameServer.Protocol;
using GameServer;
using LiteNetLib.Utils;
using System;

namespace GameFramework
{
    public class NetManager : MonoSingleton<NetManager>
    {
        private LiteNetLib.NetManager netManager;
        private EventBasedNetListener listener;
        private NetPeer server;

        public int PlayerID;
        public NetEvent NetEvent { get; private set; }
        protected override void OnDispose()
        {
            //throw new System.NotImplementedException();

            netManager.Stop();
        }

        protected override void OnInit()
        {
            MessagePackInit();

            listener = new EventBasedNetListener();

            listener.ConnectionRequestEvent += Listener_ConnectionRequestEvent;
            listener.PeerConnectedEvent += Listener_PeerConnectedEvent;
            listener.PeerDisconnectedEvent += Listener_PeerDisconnectedEvent;
            listener.NetworkReceiveEvent += Listener_NetworkReceiveEvent;

            netManager = new LiteNetLib.NetManager(listener);

            NetEvent = new NetEvent();
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
        public void Connect(string ip, int port)
        {
            netManager.Start();

            server = netManager.Connect(ip, port, "qwer123456");
        }

        public void DisConnect()
        {
            if (server != null)
            {
                server.Disconnect();
            }
            server = null;
        }

        public void Send(OperationCode code, byte[] data, DeliveryMethod delivery)
        {
            if (server == null)
            {
                return;
            }
            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put((ushort)code);
            if (data != null)
            {
                netDataWriter.Put(data);
            }
            server.Send(netDataWriter, delivery);
        }

        public void SendSyncEvent(SyncCode syncCode, byte[] data, DeliveryMethod delivery)
        {
            if (server == null)
            {
                return;
            }

            SyncRequest syncEventRequest = new SyncRequest();
            syncEventRequest.PlayerID = PlayerID;
            syncEventRequest.SyncCode = (ushort)syncCode;

            syncEventRequest.SyncData = data;
            syncEventRequest.Timestamp = DateTimeUtil.TimeStamp;

            data = MessagePackSerializer.Serialize(syncEventRequest);

            NetDataWriter netDataWriter = new NetDataWriter();
            netDataWriter.Put((ushort)OperationCode.SyncEvent);
            if (data != null)
            {
                netDataWriter.Put(data);
            }
            server.Send(netDataWriter, delivery);
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

            byte[] data = reader.GetRemainingBytes();

            //Debug.Log(string.Format("Listener_NetworkReceiveEvent {0} {1} {2}", peer.Address.ToString(), operationCode, returnCode));

            if (returnCode != ReturnCode.Success)
            {
                Debug.LogWarning(string.Format("Net Error {0} {1} {2}", peer.Address.ToString(), operationCode, returnCode));
                return;
            }

            switch (operationCode)
            {
                case OperationCode.Login:
                    break;
                case OperationCode.Register:
                    break;
                case OperationCode.JoinRoom:
                    break;
                case OperationCode.OnJoinRoom:
                    OnJoinRoom(data, deliveryMethod);

                    break;
                case OperationCode.OnOtherJoinRoom:
                    OnOtherJoinRoom(data, deliveryMethod);
                    break;
                case OperationCode.LeaveRoom:
                    break;
                case OperationCode.OnLeaveRoom:
                    OnLeaveRoom(data, deliveryMethod);
                    break;
                case OperationCode.OnOtherLeaveRoom:
                    OnOtherLeaveRoom(data, deliveryMethod);
                    break;
                case OperationCode.SyncEvent:
                    OnSyncEvent(data, deliveryMethod);
                    break;
                default:
                    break;
            }
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log(string.Format("OnPeerDisconnected {0}", peer.Address.ToString()));

            NetEvent.OnDisconnect();
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Debug.Log(string.Format("OnPeerConnected {0}", peer.Address.ToString()));

            NetEvent.OnConnect();
        }

        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Debug.Log(string.Format("OnConnectionRequest {0} {1}", request.RemoteEndPoint.ToString(), request.Data.GetString()));
        }

        private void OnJoinRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomResponse response = MessagePackSerializer.Deserialize<JoinRoomResponse>(data);
            NetEvent.OnJoinRoom(response);
        }

        private void OnOtherJoinRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            PlayerInfo playerInfoInRoom = MessagePackSerializer.Deserialize<PlayerInfo>(data);
            NetEvent.OnOtherJoinRoom(playerInfoInRoom);
        }

        private void OnLeaveRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            NetEvent.OnLeaveRoom();
        }

        private void OnOtherLeaveRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            PlayerInfo playerInfoInRoom = MessagePackSerializer.Deserialize<PlayerInfo>(data);
            NetEvent.OnOtherLeaveRoom(playerInfoInRoom);
        }

        private void OnSyncEvent(byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncRequest syncEventRequest = MessagePackSerializer.Deserialize<SyncRequest>(data);
            NetEvent.OnSync(syncEventRequest);
        }

    }
}