using Cysharp.Threading.Tasks;
using GameServer;
using GameServer.Protocol;
using MessagePack;
using Newtonsoft.Json;
using TMPro;
using UnityChan;
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

        public Button BtLock;

        public TMP_InputField inputFieldRoomID;
        public Slider slider;


        public string IP = "127.0.0.1";
        public int Port = 8888;

        public string PlayerPrefab;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            slider.value = 60;

            ResourceManager.Instance.LoadAssetBundle(Application.streamingAssetsPath + "/StandaloneWindows/prefab");
        }
        // Start is called before the first frame update
        void Start()
        {

            slider.onValueChanged.AddListener((value) =>
            {
                Application.targetFrameRate = (int)value;
            });
            

            BtConnect.onClick.AddListener(() =>
            {
                NetManager.Instance.Connect(IP, Port, Random.Range(-1000, 1000));
            });

            BtDisconnect.onClick.AddListener(() =>
            {
                NetManager.Instance.DisConnect();
            });

            BtJoinRoom.onClick.AddListener(() =>
            {
                int roomID = int.Parse(inputFieldRoomID.text);

                JoinRoomRequest request = new JoinRoomRequest();

                request.RoomID = roomID;
                request.UserID = NetManager.Instance.UserID;

                UserInfo userInfo = new UserInfo();
                userInfo.UserID = NetManager.Instance.UserID;

                request.UserInfo = userInfo;

                NetManager.Instance.SendRequest(OperationCode.JoinRoom, request);

            });

            BtLeaveRoom.onClick.AddListener(() =>
            {
                LeaveRoomRequest request = new LeaveRoomRequest();
                request.UserID = NetManager.Instance.UserID;
                NetManager.Instance.SendRequest(OperationCode.LeaveRoom, request);
            });

            BtLock.onClick.AddListener(() =>
            {
                NetComponent[] netComponents = FindObjectsByType<NetComponent>(FindObjectsSortMode.None);

                CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();

                cameraFollow.target = netComponents[Random.Range(0, netComponents.Length)].transform;

            });


            NetManager.Instance.OnConnectEvent += Instance_OnConnectEvent;

            NetManager.Instance.OnJoinRoomEvent += Instance_OnJoinRoomEvent;

            NetManager.Instance.OnLeaveRoomEvent += Instance_OnLeaveRoomEvent;

        }

        private void Instance_OnConnectEvent()
        {
            LoginRequest request = new LoginRequest();
            NetManager.Instance.SendRequest(OperationCode.Login, request);
        }

        private void Instance_OnLeaveRoomEvent(LeaveRoomResponse obj)
        {
            Debug.Log(JsonConvert.SerializeObject(obj));

            NetPoolManager.Instance.RemoveObject($"Assets/Example/Net/Prefabs/{PlayerPrefab}.prefab", obj.UserID);
        }

        private void Instance_OnJoinRoomEvent(JoinRoomResponse obj)
        {
            Debug.Log(JsonConvert.SerializeObject(obj));

            if (obj.UserID == NetManager.Instance.UserID)
            {
                NetComponent netComponent = NetPoolManager.Instance.SpawnObject($"Assets/Example/Net/Prefabs/{PlayerPrefab}.prefab", obj.UserID, obj.UserID, true);
                UnityChanControlScriptWithRgidBody playerController = netComponent.GetComponent<UnityChanControlScriptWithRgidBody>();

                playerController.enabled = true;


                for (global::System.Int32 i = 0; i < obj.Users.Count; i++)
                {
                    if (obj.Users[i].UserID != NetManager.Instance.UserID)
                        NetPoolManager.Instance.SpawnObject($"Assets/Example/Net/Prefabs/{PlayerPrefab}.prefab", obj.Users[i].UserID, obj.Users[i].UserID, false);
                }
            }
            //else
            //{
            //    NetPoolManager.Instance.SpawnObject($"Assets/Example/Net/Prefabs/{PlayerPrefab}.prefab", obj.UserID, obj.UserID);
            //}
        }
    }
}
