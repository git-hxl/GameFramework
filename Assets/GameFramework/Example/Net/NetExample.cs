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


            GameObject player = ObjectPoolManager.Instance.Acquire("Player");

            player.name = joinRoomResponse.PlayerID.ToString();

            SyncTransform syncTransform = player.GetComponent<SyncTransform>();

            syncTransform.Init(joinRoomResponse.PlayerID);

            for (int i = 0; i < joinRoomResponse.Others.Count; i++)
            {
                var playerInfoInRoom = joinRoomResponse.Others[i];

                GameObject robot = ObjectPoolManager.Instance.Acquire("Robot");

                robot.name = playerInfoInRoom.PlayerID.ToString();

                robots.Add(playerInfoInRoom.PlayerID, robot);
            }

            //robots.Add(joinRoomResponse.PlayerID, player);
        }

        public void OnLeaveRoom()
        {
            Debug.Log("OnLeaveRoom");
        }

        public void OnOtherJoinRoom(PlayerInfo playerInfoInRoom)
        {
            Debug.Log("OnOtherJoinRoom: " + playerInfoInRoom.PlayerID);

            GameObject robot = ObjectPoolManager.Instance.Acquire("Robot");

            robot.name = playerInfoInRoom.PlayerID.ToString();

            robots.Add(playerInfoInRoom.PlayerID, robot);
        }

        public void OnOtherLeaveRoom(PlayerInfo playerInfoInRoom)
        {
            Debug.Log("OnOtherLeaveRoom: " + playerInfoInRoom.PlayerID);

            GameObject robot = robots[playerInfoInRoom.PlayerID];

            ObjectPoolManager.Instance.Release("Robot", robot);

            robots.Remove(playerInfoInRoom.PlayerID);
        }


        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = 60;

            ObjectPoolManager.Instance.CreateReferenceCollection("Robot", "Assets/GameFramework/Example/Net/Prefabs/Robot.prefab");

            ObjectPoolManager.Instance.CreateReferenceCollection("Player", "Assets/GameFramework/Example/Net/Prefabs/Player.prefab");

            NetManager.Instance.NetEvent.Register(this);

            NetManager.Instance.NetEvent.OnSyncTransformEvent += NetEvent_OnSyncTransformEvent;

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

        private void NetEvent_OnSyncTransformEvent(int playerID, long timestamp, SyncTransformData data)
        {
            if (playerID == 0)
                return;

            if (robots.ContainsKey(playerID))
            {
                GameObject robot = robots[playerID];

                SyncTransform syncTransform = robot.GetComponent<SyncTransform>();

                syncTransform.AddData(timestamp, data);
            }
            else
            {

                GameObject robot = ObjectPoolManager.Instance.Acquire("Robot");

                SyncTransform syncTransform = robot.GetComponent<SyncTransform>();
                robot.name = playerID.ToString();
                robots.Add(playerID, robot);

                syncTransform.AddData(timestamp, data);
            }
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
