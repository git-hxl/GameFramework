
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
        public int UserID { get; private set; }
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
        public void Connect(string ip, int port, int userID)
        {
            UserID = userID;

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

        public void SendRequest(OperationCode code, BaseRequest baseRequest, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
        {
            if (server == null)
                return;

            NetDataWriter netDataWriter = new NetDataWriter();

            netDataWriter.Put((ushort)code);

            if (baseRequest == null)
            {
                baseRequest = new BaseRequest();
            }

            baseRequest.UserID = UserID;
            baseRequest.Timestamp = DateTimeUtil.TimeStamp;

            Type type = baseRequest.GetType();

            byte[] data = MessagePackSerializer.Serialize(type, baseRequest);

            netDataWriter.Put(data);

            server.Send(netDataWriter, deliveryMethod);
        }


        private void Listener_NetworkReceiveEvent(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            try
            {
                OperationCode operationCode = (OperationCode)reader.GetUShort();

                ReturnCode returnCode = (ReturnCode)reader.GetUShort();

                Debug.Log(string.Format("接收请求：{0} return {1} ping {2}", operationCode, returnCode, peer.Ping));

                byte[] data = reader.GetRemainingBytes();

                if (returnCode != ReturnCode.Success)
                {
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

        private void OnJoinRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            JoinRoomResponse joinRoomResponse = MessagePackSerializer.Deserialize<JoinRoomResponse>(data);
            OnJoinRoomEvent?.Invoke(joinRoomResponse);
        }


        private void OnLeaveRoom(byte[] data, DeliveryMethod deliveryMethod)
        {
            LeaveRoomResponse leaveRoomResponse = MessagePackSerializer.Deserialize<LeaveRoomResponse>(data);
            OnLeaveRoomEvent?.Invoke(leaveRoomResponse);
        }


        private void OnUpdatePlayerInfo(byte[] data, DeliveryMethod deliveryMethod)
        {
            UserInfo userInfo = MessagePackSerializer.Deserialize<UserInfo>(data);
            OnUpdatePlayerInfoEvent?.Invoke(userInfo);
        }

        private void OnUpdateRoomInfo(byte[] data, DeliveryMethod deliveryMethod)
        {
            RoomInfo roomInfo = MessagePackSerializer.Deserialize<RoomInfo>(data);
            OnUpdateRoomInfoEvent?.Invoke(roomInfo);
        }

        private void OnSyncEvent(byte[] data, DeliveryMethod deliveryMethod)
        {
            SyncRequest syncRequest = MessagePackSerializer.Deserialize<SyncRequest>(data);

            OnSyncRequestEvent?.Invoke(syncRequest);

            Debug.Log(string.Format("接收Sync请求：{0}", syncRequest.SyncCode));

            switch (syncRequest.SyncCode)
            {
                case SyncCode.SyncSpawnObject:
                    ObjectData objectData = MessagePackSerializer.Deserialize<ObjectData>(syncRequest.SyncData);

                    OnSyncSpawnObjectEvent(syncRequest, objectData);
                    break;

                case SyncCode.SyncRemoveObject:
                    objectData = MessagePackSerializer.Deserialize<ObjectData>(syncRequest.SyncData);

                    OnSyncRemoveObjectEvent(syncRequest, objectData);
                    break;

                case SyncCode.SyncTransform:
                    TransformData transformData = MessagePackSerializer.Deserialize<TransformData>(syncRequest.SyncData);

                    OnSyncTransformEvent(syncRequest, transformData);
                    break;

                case SyncCode.SyncAnimation:
                    AnimationData animationData = MessagePackSerializer.Deserialize<AnimationData>(syncRequest.SyncData);

                    OnSyncAnimationEvent(syncRequest, animationData);
                    break;
            }
        }

    }
}