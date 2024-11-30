using Cysharp.Threading.Tasks;
using GameServer;
using GameServer.Protocol;
using MessagePack;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public class NetExample : MonoBehaviour, INetEvent
    {
        public Button BtConnect;
        public Button BtDisconnect;
        public Button BtJoinRoom;
        public Button BtLeaveRoom;

        public Button BtRobitTest;

        public TMP_InputField inputFieldRoomID;

        private Dictionary<int, GameObject> robots = new Dictionary<int, GameObject>();
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

            //for (int i = 0; i < joinRoomResponse.Others.Count; i++)
            //{
            //    PlayerInfoInRoom playerInfoInRoom = joinRoomResponse.Others[i];

            //    GameObject robot = ObjectPoolManager.Instance.Acquire("Robot");

            //    robot.name = playerInfoInRoom.PlayerID.ToString();

            //    robots.Add(playerInfoInRoom.PlayerID, robot);
            //}

            GameObject player = ObjectPoolManager.Instance.Acquire("Player");

            player.name = joinRoomResponse.PlayerID.ToString();

            SyncTransform syncTransform = player.GetComponent<SyncTransform>();

            syncTransform.Init(joinRoomResponse.PlayerID);

            //robots.Add(joinRoomResponse.PlayerID, player);
        }

        public void OnLeaveRoom()
        {
            Debug.Log("OnLeaveRoom");
        }

        public void OnOtherJoinRoom(PlayerInfoInRoom playerInfoInRoom)
        {
            Debug.Log("OnOtherJoinRoom: " + playerInfoInRoom.PlayerID);

            GameObject robot = ObjectPoolManager.Instance.Acquire("Robot");

            robot.name = playerInfoInRoom.PlayerID.ToString();

            robots.Add(playerInfoInRoom.PlayerID, robot);
        }

        public void OnOtherLeaveRoom(PlayerInfoInRoom playerInfoInRoom)
        {
            Debug.Log("OnOtherLeaveRoom: " + playerInfoInRoom.PlayerID);

            GameObject robot = robots[playerInfoInRoom.PlayerID];

            ObjectPoolManager.Instance.Release("Robot", robot);

            robots.Remove(playerInfoInRoom.PlayerID);
        }

        public void OnSyncEvent(SyncEventRequest syncEventRequest)
        {
           // Debug.Log("OnSyncEvent");

            switch ((EventCode)syncEventRequest.EventID)
            {
                case EventCode.SyncTransform:

                    SyncTransformData syncTransformData = MessagePackSerializer.Deserialize<SyncTransformData>(syncEventRequest.SyncData);

                    if(robots.ContainsKey(syncEventRequest.PlayerID))
                    {
                        GameObject robot = robots[syncEventRequest.PlayerID];

                        SyncTransform syncTransform = robot.GetComponent<SyncTransform>();
                        syncTransform.OnReceiveRemoteData(syncEventRequest.Timestamp, syncTransformData);

                    }
                    else
                    {

                        GameObject robot = ObjectPoolManager.Instance.Acquire("Robot");

                        SyncTransform syncTransform = robot.GetComponent<SyncTransform>();
                        robot.name = syncEventRequest.PlayerID.ToString();
                        robots.Add(syncEventRequest.PlayerID, robot);

                        syncTransform.OnReceiveRemoteData(syncEventRequest.Timestamp, syncTransformData);
                    }
                    break;
            }
        }

        // Start is called before the first frame update
        void Start()
        {

            ObjectPoolManager.Instance.CreateReferenceCollection("Robot", "Assets/GameFramework/Example/Net/Prefabs/Robot.prefab");

            ObjectPoolManager.Instance.CreateReferenceCollection("Player", "Assets/GameFramework/Example/Net/Prefabs/Player.prefab");

            NetEvent.Instance.Register(this);

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
                joinRoomRequest.PlayerID = NetManager.Instance.PlayerID;
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

            BtRobitTest.onClick.AddListener(() => { RobitJoinTest().Forget(); RobitLeaveTest().Forget(); });
        }

        private async UniTask RobitJoinTest()
        {
            while (true)
            {
                await UniTask.Delay(100);
                JoinRoomRequest joinRoomRequest = new JoinRoomRequest();
                joinRoomRequest.PlayerID = Random.Range(1, 100);

                joinRoomRequest.RoomID = Random.Range(1, 10);
                byte[] data = MessagePackSerializer.Serialize(joinRoomRequest);
                NetManager.Instance.Send(OperationCode.JoinRoom, data, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }
        }

        private async UniTask RobitLeaveTest()
        {
            while (true)
            {
                await UniTask.Delay(100);
                LeaveRoomRequest leaveRoomRequest = new LeaveRoomRequest();
                leaveRoomRequest.PlayerID = Random.Range(1, 100);
                byte[] data = MessagePackSerializer.Serialize(leaveRoomRequest);
                NetManager.Instance.Send(OperationCode.LeaveRoom, data, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
