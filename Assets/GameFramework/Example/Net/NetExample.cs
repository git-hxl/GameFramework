using Cysharp.Threading.Tasks;
using GameServer;
using GameServer.Protocol;
using MessagePack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameFramework
{
    public class NetExample : MonoBehaviour
    {
        public Button BtConnect;
        public Button BtDisconnect;
        public Button BtJoinRoom;
        public Button BtLeaveRoom;

        public Button BtRobitTest;

        public TMP_InputField inputFieldRoomID;

        private NetComponent self;
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

            if (joinRoomResponse.PlayerID == NetManager.Instance.PlayerID)
            {
                self = NetPoolManager.Instance.SpawnLocalObject("Player");
            }

            //ObjectPoolManager.Instance.Acquire("Player");

            //for (int i = 0; i < joinRoomResponse.PlayerInfos.Count; i++)
            //{
            //    var playerInfoInRoom = joinRoomResponse.PlayerInfos[i];

            //    NetPoolManager.Instance.SpawnObject("Robot", playerInfoInRoom.PlayerID);
            //}
        }

        public void OnLeaveRoom(LeaveRoomResponse leaveRoomResponse)
        {
            Debug.Log("OnLeaveRoom:" + leaveRoomResponse.PlayerID);

            //NetPoolManager.Instance.UnspawnAllObjects(leaveRoomResponse.PlayerID, "Robot");
        }


        // Start is called before the first frame update
        void Start()
        {
            //Application.targetFrameRate = 60;

            ObjectPoolManager.Instance.CreateReferenceCollection("Player_Remote", "Assets/GameFramework/Example/Net/Prefabs/Robot.prefab");

            ObjectPoolManager.Instance.CreateReferenceCollection("Player", "Assets/GameFramework/Example/Net/Prefabs/Player.prefab");

            NetManager.Instance.OnConnectEvent += OnConnect;
            NetManager.Instance.OnDisconnectEvent += OnDisconnect;
            NetManager.Instance.OnJoinRoomEvent += OnJoinRoom;
            NetManager.Instance.OnLeaveRoomEvent += OnLeaveRoom;

            BtConnect.onClick.AddListener(() =>
            {
                NetManager.Instance.Connect("127.0.0.1", 8888, -1);
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
                NetPoolManager.Instance.RemoveLocalObject(self);
                LeaveRoomRequest leaveRoomRequest = new LeaveRoomRequest();
                leaveRoomRequest.PlayerID = NetManager.Instance.PlayerID;
                byte[] data = MessagePackSerializer.Serialize(leaveRoomRequest);
                NetManager.Instance.Send(OperationCode.LeaveRoom, data, LiteNetLib.DeliveryMethod.ReliableOrdered);
            });

            //BtRobitTest.onClick.AddListener(() => { RobitJoinTest().Forget(); RobitLeaveTest().Forget(); });
        }

        private void Instance_OnConnectEvent()
        {
            throw new System.NotImplementedException();
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
