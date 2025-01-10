
using MessagePack.Resolvers;
using MessagePack;
using UnityEngine;
using LiteNetLib;
using GameServer.Protocol;
using GameServer;
using LiteNetLib.Utils;

namespace GameFramework
{
    public partial class NetManager : MonoSingleton<NetManager>
    {
        private LiteNetLib.NetManager netManager;
        private EventBasedNetListener listener;
        private NetPeer server;
        /// <summary>
        /// 唯一性ID,作本机识别用
        /// </summary>
        public int PlayerID { get; private set; }
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
        public void Connect(string ip, int port, int playerID)
        {
            PlayerID = playerID;

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
            Debug.Log("发送字节大小：" + data.Length);

            server.Send(netDataWriter, delivery);
        }

        public void SendSyncEvent(SyncEventCode syncCode, byte[] data, DeliveryMethod delivery)
        {
            if (server == null)
            {
                return;
            }

            SyncRequestData syncEventRequest = new SyncRequestData();
            syncEventRequest.PlayerID = PlayerID;
            syncEventRequest.SyncEventCode = (ushort)syncCode;

            syncEventRequest.SyncData = data;
            syncEventRequest.Timestamp = DateTimeUtil.TimeStamp;

            data = MessagePackSerializer.Serialize(syncEventRequest);

            Send(OperationCode.SyncEvent, data, delivery);
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
                    OnJoinRoom(data, deliveryMethod);
                    break;
                case OperationCode.LeaveRoom:
                    OnLeaveRoom(data, deliveryMethod);
                    break;

                case OperationCode.UpdateRoomInfo:
                    OnUpdateRoomInfo(data, deliveryMethod);
                    break;
                case OperationCode.UpdatePlayerInfo:
                    OnUpdatePlayerInfo(data, deliveryMethod);
                    break;


                case OperationCode.SyncEvent:
                    OnSyncEvent(data, deliveryMethod);
                    break;
                default:
                    Debug.LogError("不存在的操作码:" + operationCode);
                    break;
            }
        }

        private void Listener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log(string.Format("OnPeerDisconnected {0}", peer.Address.ToString()));

            OnDisconnectEvent?.Invoke();
        }

        private void Listener_PeerConnectedEvent(NetPeer peer)
        {
            Debug.Log(string.Format("OnPeerConnected {0}", peer.Address.ToString()));

            OnConnectEvent?.Invoke();
        }

        private void Listener_ConnectionRequestEvent(ConnectionRequest request)
        {
            Debug.Log(string.Format("OnConnectionRequest {0} {1}", request.RemoteEndPoint.ToString(), request.Data.GetString()));
        }

        private void OnJoinRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomResponse response = MessagePackSerializer.Deserialize<JoinRoomResponse>(data);
            OnJoinRoomEvent?.Invoke(response);
        }


        private void OnLeaveRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            LeaveRoomResponse leaveRoomResponse = MessagePackSerializer.Deserialize<LeaveRoomResponse>(data);
            OnLeaveRoomEvent?.Invoke(leaveRoomResponse);
        }


        private void OnUpdatePlayerInfo(byte[] data, DeliveryMethod deliveryMethod)
        {
            PlayerInfo playerInfo = MessagePackSerializer.Deserialize<PlayerInfo>(data);
            OnUpdatePlayerInfoEvent?.Invoke(playerInfo);
        }

        private void OnUpdateRoomInfo(byte[] data, DeliveryMethod deliveryMethod)
        {
            RoomInfo roomInfo = MessagePackSerializer.Deserialize<RoomInfo>(data);
            OnUpdateRoomInfoEvent?.Invoke(roomInfo);
        }

        private void OnSyncEvent(byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncRequestData syncRequestData = MessagePackSerializer.Deserialize<SyncRequestData>(data);

            OnSyncRequestEvent?.Invoke(syncRequestData);

            switch (syncRequestData.SyncEventCode)
            {

            }

        }

    }
}