
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
    public partial class NetManager : MonoSingleton<NetManager>
    {
        private LiteNetLib.NetManager netManager;
        private EventBasedNetListener listener;
        private NetPeer server;

        public NetPeer Server { get { return server; } }
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

#if UNITY_EDITOR
        void OnGUI()
        {
            if (server != null)
            {
                GUILayout.TextField($"Ping {server.Ping} MTU {server.Mtu} Interval {server.TimeSinceLastPacket} RemoteDelta {server.RemoteTimeDelta}");
            }
        }
#endif

        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            try
            {
                Response response = MessagePackSerializer.Deserialize<Response>(reader.GetRemainingBytes());

                Debug.Log($"接收数据：{response.OperationCode} {response.ReturnCode} {response.ErrorMsg} 延迟：{DateTimeUtil.TimeStamp - response.Timestamp} ping：{peer.Ping}");

                if (response.ReturnCode != ReturnCode.Success)
                {
                    return;
                }

                switch (response.OperationCode)
                {
                    case OperationCode.Login:
                        break;
                    case OperationCode.Register:
                        break;
                    case OperationCode.JoinRoom:
                        OnJoinRoom(response, deliveryMethod);
                        break;
                    case OperationCode.LeaveRoom:
                        OnLeaveRoom(response, deliveryMethod);
                        break;

                    case OperationCode.UpdateRoomInfo:
                        OnUpdateRoomInfo(response, deliveryMethod);
                        break;
                    case OperationCode.UpdatePlayerInfo:
                        OnUpdatePlayerInfo(response, deliveryMethod);
                        break;


                    case OperationCode.SyncEvent:
                        OnSyncEvent(response, deliveryMethod);
                        break;
                    default:
                        Debug.LogError("不存在的操作码:" + response.OperationCode);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
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

        private void OnJoinRoom(Response response, DeliveryMethod deliveryMethod)
        {
            JoinRoomResponse joinRoomResponse = MessagePackSerializer.Deserialize<JoinRoomResponse>(response.Data);
            OnJoinRoomEvent?.Invoke(joinRoomResponse);
        }


        private void OnLeaveRoom(Response response, DeliveryMethod deliveryMethod)
        {
            LeaveRoomResponse leaveRoomResponse = MessagePackSerializer.Deserialize<LeaveRoomResponse>(response.Data);
            OnLeaveRoomEvent?.Invoke(leaveRoomResponse);
        }


        private void OnUpdatePlayerInfo(Response response, DeliveryMethod deliveryMethod)
        {
            PlayerInfo playerInfo = MessagePackSerializer.Deserialize<PlayerInfo>(response.Data);
            OnUpdatePlayerInfoEvent?.Invoke(playerInfo);
        }

        private void OnUpdateRoomInfo(Response response, DeliveryMethod deliveryMethod)
        {
            RoomInfo roomInfo = MessagePackSerializer.Deserialize<RoomInfo>(response.Data);
            OnUpdateRoomInfoEvent?.Invoke(roomInfo);
        }

        private void OnSyncEvent(Response response, DeliveryMethod deliveryMethod)
        {
            SyncEventRequest syncRequestData = MessagePackSerializer.Deserialize<SyncEventRequest>(response.Data);

            OnSyncRequestEvent?.Invoke(response.Timestamp, syncRequestData);

            switch (syncRequestData.SyncEventCode)
            {

            }

        }

    }
}