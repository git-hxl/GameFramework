using GameServer;
using GameServer.Protocol;
using LiteNetLib.Utils;
using MessagePack;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameFramework
{
    public class NetExample : MonoBehaviour, INetEvent
    {
        public Button BtConnect;
        public Button BtDisconnect;
        public Button BtJoinRoom;
        public Button BtLeaveRoom;

        public TMP_InputField inputFieldRoomID;
        public void OnConnect()
        {
            Debug.Log("OnConnect");
        }

        public void OnDisconnect()
        {
            Debug.Log("OnDisconnect");
        }

        public void OnJoinRoom(JoinRoomResponse joinRoomResponse)
        {
            Debug.Log("OnJoinRoom");
        }

        public void OnLeaveRoom()
        {
            Debug.Log("OnLeaveRoom");
        }

        public void OnOtherJoinRoom(PlayerInfoInRoom playerInfoInRoom)
        {
            Debug.Log("OnOtherJoinRoom");
        }

        public void OnOtherLeaveRoom(PlayerInfoInRoom playerInfoInRoom)
        {
            Debug.Log("OnOtherLeaveRoom");
        }

        public void OnSyncEvent(SyncEventData eventData)
        {
            Debug.Log("OnSyncEvent");
        }

        // Start is called before the first frame update
        void Start()
        {
            NetEvent.Register(this);



            BtConnect.onClick.AddListener(() =>
            {
                NetManager.Instance.Connect("127.0.0.1", 1111);
            });

            BtDisconnect.onClick.AddListener(() =>
            {
                NetManager.Instance.DisConnect();
            });

            BtJoinRoom.onClick.AddListener(() =>
            {
                JoinRoomRequest joinRoomRequest = new JoinRoomRequest();
                joinRoomRequest.PlayerID = 123;
                int roomID = int.Parse(inputFieldRoomID.text);
                joinRoomRequest.RoomID = roomID;
                byte[] data = MessagePackSerializer.Serialize(joinRoomRequest);
                NetManager.Instance.Send(OperationCode.JoinRoom, data, LiteNetLib.DeliveryMethod.ReliableOrdered);
            });

            BtLeaveRoom.onClick.AddListener(() =>
            {
                LeaveRoomRequest leaveRoomRequest = new LeaveRoomRequest();
                leaveRoomRequest.PlayerID = 123;
                byte[] data = MessagePackSerializer.Serialize(leaveRoomRequest);
                NetManager.Instance.Send(OperationCode.LeaveRoom, data, LiteNetLib.DeliveryMethod.ReliableOrdered);
            });
        }
    }
}
