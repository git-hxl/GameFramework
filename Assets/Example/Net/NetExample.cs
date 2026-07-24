using System;
using System.Collections.Generic;
using GameFramework;
using SharedLib.Models;
using UnityEngine;

public class NetExample : MonoBehaviour
{
    const int ServerPort = 6001;

    LobbyClient _lobbyClient;
    GameClient _gameClient;

    PlayerInfo _player;
    GameObject _localCharacter;
    Dictionary<long, GameObject> _spawnedObjects = new();
    Dictionary<long, EntitySyncTarget> _entityTargets = new();
    List<string> _logLines = new();
    string _inputText = "";
    Vector2 _scrollPos;
    bool _autoScroll = true;

    string _gsAddress = "127.0.0.1";
    int _gsPort = 7001;

    float _syncTimer;
    float _syncInterval = 0.1f;
    Animator _localAnimator;

    Vector3 _lastPos;
    Quaternion _lastRot;

    int _lastAnimHash;
    float _lastAnimTime;
    Dictionary<string, int> _lastIntParams = new();
    Dictionary<string, float> _lastFloatParams = new();
    Dictionary<string, bool> _lastBoolParams = new();

    void Awake()
    {
        ResourceManager.Instance.LoadAssetBundle(Application.streamingAssetsPath + "/StandaloneOSX/" + "prefab");

        var userId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _player = new PlayerInfo
        {
            UserId = userId,
            Nickname = $"Test_{userId % 10000}",
            Gender = 1,
            Avatar = "default.png",
            Age = 20
        };

        _lobbyClient = new LobbyClient();
        _lobbyClient.OnLog += AddLog;
        _lobbyClient.OnGameServerAssigned += (addr, port) =>
        {
            _gsAddress = addr;
            _gsPort = port;
            _gameClient.Connect(addr, port);
        };

        _gameClient = new GameClient();
        _gameClient.OnLog += AddLog;
        _gameClient.OnJoinedGame += OnJoinedGameRoom;
        _gameClient.OnLeftGame += ClearAllGameObjects;
        _gameClient.OnPlayerJoinedGame += SpawnPlayerCharacter;
        _gameClient.OnPlayerLeftGame += DespawnPlayerCharacter;
        _gameClient.OnObjectSpawnReceived += OnObjectSpawn;
        _gameClient.OnObjectDespawnReceived += OnObjectDespawn;
        _gameClient.OnPositionSyncReceived += OnPositionSync;
        _gameClient.OnAnimationSyncReceived += OnAnimationSync;
    }

    void Start()
    {
        _lobbyClient.Start();
        _gameClient.Start();
    }

    void Update()
    {
        _lobbyClient.PollEvents();
        _gameClient.PollEvents();

        float dt = Time.deltaTime;
        if (_localCharacter != null)
        {
            _syncTimer += dt;
            if (_syncTimer >= _syncInterval)
            {
                _syncTimer = 0f;
                SendPositionSync();
                SendAnimationSync();
            }
        }

        float t = Mathf.Clamp01(dt / _syncInterval * 2f);
        foreach (var kv in _entityTargets)
        {
            if (kv.Key == _player.UserId)
                continue;
            if (!_spawnedObjects.TryGetValue(kv.Key, out var go) || go == null)
                continue;
            var target = kv.Value;

            float maxSpeed = Vector3.Distance(go.transform.position, target.Position) / _syncInterval;
            go.transform.position = Vector3.MoveTowards(go.transform.position, target.Position, maxSpeed * dt);
            go.transform.rotation = Quaternion.Slerp(go.transform.rotation, target.Rotation, t);
        }
    }

    void OnDestroy()
    {
        _lobbyClient?.Stop();
        _gameClient?.Stop();
    }

    void AddLog(string msg)
    {
        _logLines.Add(msg);
        if (_logLines.Count > 500)
            _logLines.RemoveAt(0);
    }

    void OnJoinedGameRoom(JoinGameResponse resp)
    {
        if (resp.Players == null) return;

        var localPrefab = ResourceManager.Instance.LoadAsset<GameObject>("Assets/UnityTechnologies/SpaceRobotKyle/Prefabs/RobotKyle.prefab");
        var remotePrefab = ResourceManager.Instance.LoadAsset<GameObject>("Assets/UnityTechnologies/SpaceRobotKyle/Prefabs/RobotKyle 1.prefab");
        if (localPrefab == null || remotePrefab == null)
        {
            AddLog($"[Game] Failed to load player prefab");
            return;
        }

        foreach (var p in resp.Players)
        {
            if (_spawnedObjects.ContainsKey(p.UserId))
                continue;

            var isLocal = p.UserId == _player.UserId;
            var go = Instantiate(isLocal ? localPrefab : remotePrefab);
            go.name = $"Player_{p.UserId}";
            _spawnedObjects[p.UserId] = go;

            if (isLocal)
            {
                _localCharacter = go;
                _localAnimator = go.GetComponentInChildren<Animator>();
            }
            else
            {
                var input = go.GetComponent<StarterAssets.StarterAssetsInputs>();
                if (input != null) input.cursorLocked = false;
                var playerInput = go.GetComponent<UnityEngine.InputSystem.PlayerInput>();
                if (playerInput != null) playerInput.enabled = false;
            }
        }
        AddLog($"[Game] Spawned {resp.Players.Count} player(s) in room {resp.RoomId}");
    }

    static readonly Dictionary<string, string> PrefabPaths = new()
    {
        { "RobotKyle", "Assets/UnityTechnologies/SpaceRobotKyle/Prefabs/RobotKyle.prefab" }
    };

    void OnObjectSpawn(ObjectSpawnData data)
    {
        if (_spawnedObjects.TryGetValue(data.ObjectId, out var existGo))
        {
            existGo.transform.SetPositionAndRotation(
                new Vector3(data.PosX, data.PosY, data.PosZ),
                Quaternion.Euler(data.RotX, data.RotY, data.RotZ));
            return;
        }

        if (!PrefabPaths.TryGetValue(data.PrefabName, out var path))
        {
            AddLog($"[Game] Unknown prefab: {data.PrefabName}");
            return;
        }

        var prefab = ResourceManager.Instance.LoadAsset<GameObject>(path);
        if (prefab == null)
        {
            AddLog($"[Game] Failed to load prefab: {path}");
            return;
        }

        var go = Instantiate(prefab);
        go.name = $"{data.PrefabName}_{data.ObjectId}";
        go.transform.SetPositionAndRotation(
            new Vector3(data.PosX, data.PosY, data.PosZ),
            Quaternion.Euler(data.RotX, data.RotY, data.RotZ));
        _spawnedObjects[data.ObjectId] = go;
    }

    void OnObjectDespawn(ObjectDespawnData data)
    {
        _entityTargets.Remove(data.ObjectId);
        if (_spawnedObjects.TryGetValue(data.ObjectId, out var go))
        {
            Destroy(go);
            _spawnedObjects.Remove(data.ObjectId);
        }
    }

    void OnPositionSync(PositionSyncData data)
    {
        if (!_spawnedObjects.ContainsKey(data.EntityId))
            return;

        _entityTargets[data.EntityId] = new EntitySyncTarget
        {
            Position = new Vector3(data.PosX, data.PosY, data.PosZ),
            Rotation = Quaternion.Euler(data.RotX, data.RotY, data.RotZ)
        };

        AddLog($"[Game] PositionSync: id={data.EntityId} pos=({data.PosX:F2},{data.PosY:F2},{data.PosZ:F2})");
    }

    void OnAnimationSync(AnimationSyncData data)
    {
        if (!_spawnedObjects.TryGetValue(data.EntityId, out var go) || go == null)
            return;

        ApplyAnimSync(go, data.AnimName, data.AnimNormalTime);
        ApplyAnimParams(go, data.IntParams, data.FloatParams, data.BoolParams);

        AddLog($"[Game] AnimationSync: id={data.EntityId} hash={data.AnimName} time={data.AnimNormalTime:F2}");
    }

    void SpawnPlayerCharacter(PlayerInfo playerInfo)
    {
        if (_spawnedObjects.ContainsKey(playerInfo.UserId))
            return;

        var path = "Assets/UnityTechnologies/SpaceRobotKyle/Prefabs/RobotKyle 1.prefab";
        var prefab = ResourceManager.Instance.LoadAsset<GameObject>(path);
        if (prefab == null)
        {
            AddLog($"[Game] Failed to load prefab for player {playerInfo.Nickname}");
            return;
        }

        var go = Instantiate(prefab);
        go.name = $"Player_{playerInfo.UserId}";
        _spawnedObjects[playerInfo.UserId] = go;
        var input = go.GetComponent<StarterAssets.StarterAssetsInputs>();
        if (input != null) input.cursorLocked = false;
        var playerInput = go.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;
        AddLog($"[Game] Spawned player: {playerInfo.Nickname} (id={playerInfo.UserId})");
    }

    void DespawnPlayerCharacter(PlayerInfo playerInfo)
    {
        _entityTargets.Remove(playerInfo.UserId);
        if (_spawnedObjects.TryGetValue(playerInfo.UserId, out var go))
        {
            Destroy(go);
            _spawnedObjects.Remove(playerInfo.UserId);
            AddLog($"[Game] Despawned player: {playerInfo.Nickname} (id={playerInfo.UserId})");
        }
    }

    void ClearAllGameObjects()
    {
        foreach (var kv in _spawnedObjects)
        {
            if (kv.Value != null)
                Destroy(kv.Value);
        }
        _spawnedObjects.Clear();
        _entityTargets.Clear();
        _localCharacter = null;
        _localAnimator = null;
        AddLog("[Game] Cleared all game objects");
    }

    void SendPositionSync()
    {
        var t = _localCharacter.transform;
        var pos = t.position;
        var rot = t.rotation;

        if (Vector3.Distance(pos, _lastPos) < 0.001f && Quaternion.Angle(rot, _lastRot) < 0.1f)
            return;

        _lastPos = pos;
        _lastRot = rot;

        var angles = rot.eulerAngles;
        var data = new PositionSyncData
        {
            EntityId = _player.UserId,
            EntityType = 0,
            PosX = pos.x,
            PosY = pos.y,
            PosZ = pos.z,
            RotX = angles.x,
            RotY = angles.y,
            RotZ = angles.z
        };

        _gameClient.SendPositionSync(data);
    }

    void SendAnimationSync()
    {
        if (_localAnimator == null)
            return;

        var stateInfo = _localAnimator.GetCurrentAnimatorStateInfo(0);
        int hash = stateInfo.fullPathHash;
        float time = stateInfo.normalizedTime;

        if (hash == _lastAnimHash)
        {
            bool changed = false;
            foreach (var p in _localAnimator.parameters)
                changed |= HasParamChanged(p);
            if (!changed)
                return;
        }

        _lastAnimHash = hash;
        _lastAnimTime = time;

        var data = new AnimationSyncData
        {
            EntityId = _player.UserId,
            AnimName = hash.ToString(),
            AnimNormalTime = time,

            IntParams = new Dictionary<string, int>(),
            FloatParams = new Dictionary<string, float>(),
            BoolParams = new Dictionary<string, bool>(),
        };

        foreach (var p in _localAnimator.parameters)
        {
            switch (p.type)
            {
                case AnimatorControllerParameterType.Int:
                    var iv = _localAnimator.GetInteger(p.nameHash);
                    _lastIntParams[p.name] = iv;
                    data.IntParams[p.name] = iv;
                    break;
                case AnimatorControllerParameterType.Float:
                    var fv = _localAnimator.GetFloat(p.nameHash);
                    _lastFloatParams[p.name] = fv;
                    data.FloatParams[p.name] = fv;
                    break;
                case AnimatorControllerParameterType.Bool:
                    var bv = _localAnimator.GetBool(p.nameHash);
                    _lastBoolParams[p.name] = bv;
                    data.BoolParams[p.name] = bv;
                    break;
            }
        }

        _gameClient.SendAnimationSync(data);
    }

    bool HasParamChanged(AnimatorControllerParameter p)
    {
        switch (p.type)
        {
            case AnimatorControllerParameterType.Int:
                var iv = _localAnimator.GetInteger(p.nameHash);
                if (!_lastIntParams.TryGetValue(p.name, out var lastIv) || lastIv != iv)
                    return true;
                break;
            case AnimatorControllerParameterType.Float:
                var fv = _localAnimator.GetFloat(p.nameHash);
                if (!_lastFloatParams.TryGetValue(p.name, out var lastFv) || Mathf.Abs(lastFv - fv) > 0.001f)
                    return true;
                break;
            case AnimatorControllerParameterType.Bool:
                var bv = _localAnimator.GetBool(p.nameHash);
                if (!_lastBoolParams.TryGetValue(p.name, out var lastBv) || lastBv != bv)
                    return true;
                break;
        }
        return false;
    }


    void OnGUI()
    {
        var btnW = 170f;
        var btnH = 28f;
        float y;

        // ── LobbyServer Column ──

        var lbX = 10f;
        y = 30f;
        GUI.Label(new Rect(lbX, 5, 200, 25), "═══ LobbyServer ═══");

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "Connect"))
        {
            _lobbyClient.Connect("127.0.0.1", ServerPort);
        }
        y += btnH + 3;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "Disconnect"))
        {
            _lobbyClient.Disconnect();
        }
        y += btnH + 3;

        GUI.Label(new Rect(lbX, y, btnW, 20), $"ping:{_lobbyClient.Ping}ms  mtu:{_lobbyClient.Mtu}");
        y += btnH + 3;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "JoinLobby"))
        {
            _lobbyClient.SendJoinLobby(_player);
        }
        y += btnH + 3;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "LeaveLobby"))
        {
            _lobbyClient.SendLeaveLobby(_player.UserId);
        }
        y += btnH + 8;

        _inputText = GUI.TextField(new Rect(lbX, y, btnW, btnH), _inputText);
        y += btnH + 3;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "Chat"))
        {
            var text = string.IsNullOrEmpty(_inputText) ? "Hello from test!" : _inputText;
            _lobbyClient.SendChat(_player.UserId, _player.Nickname, text);
        }
        y += btnH + 8;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "CreateRoom"))
        {
            var roomId = string.IsNullOrEmpty(_inputText) ? "test_room" : _inputText;
            _lobbyClient.SendCreateRoom(roomId);
        }
        y += btnH + 3;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "JoinRoom"))
        {
            var roomId = string.IsNullOrEmpty(_inputText) ? "test_room" : _inputText;
            _lobbyClient.SendJoinRoom(roomId);
        }
        y += btnH + 3;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "LeaveRoom"))
        {
            _lobbyClient.SendLeaveRoom();
        }
        y += btnH + 8;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "RoomList"))
        {
            _lobbyClient.SendRoomList();
        }
        y += btnH + 8;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "GameReady"))
        {
            _lobbyClient.SendGameReady();
        }
        y += btnH + 3;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "GameUnready"))
        {
            _lobbyClient.SendGameUnready();
        }
        y += btnH + 8;

        if (GUI.Button(new Rect(lbX, y, btnW, btnH), "GameStart"))
        {
            _lobbyClient.SendGameStart();
        }
        y += btnH + 3;

        // ── GameServer Column ──

        var gsX = 190f;
        y = 30f;
        GUI.Label(new Rect(gsX, 5, 200, 25), "═══ GameServer ═══");

        if (GUI.Button(new Rect(gsX, y, btnW, btnH), "Connect"))
        {
            _gameClient.Connect(_gsAddress, _gsPort);
        }
        y += btnH + 3;

        if (GUI.Button(new Rect(gsX, y, btnW, btnH), "Disconnect"))
        {
            _gameClient.Disconnect();
        }
        y += btnH + 3;

        GUI.Label(new Rect(gsX, y, btnW, 20), $"ping:{_gameClient.Ping}ms  mtu:{_gameClient.Mtu}");
        y += btnH + 3;

        GUI.Label(new Rect(gsX, y, 170, 20), "Address:");
        y += 18;
        _gsAddress = GUI.TextField(new Rect(gsX, y, btnW, btnH), _gsAddress);
        y += btnH + 3;

        GUI.Label(new Rect(gsX, y, 170, 20), "Port:");
        y += 18;
        var portStr = GUI.TextField(new Rect(gsX, y, btnW, btnH), _gsPort.ToString());
        if (int.TryParse(portStr, out var p)) _gsPort = p;
        y += btnH + 8;

        if (GUI.Button(new Rect(gsX, y, btnW, btnH), "JoinGame"))
        {
            var roomId = string.IsNullOrEmpty(_inputText) ? "test_room" : _inputText;
            _gameClient.SendJoinGame(roomId, _player);
        }
        y += btnH + 3;

        if (GUI.Button(new Rect(gsX, y, btnW, btnH), "LeaveGame"))
        {
            _gameClient.SendLeaveGame();
        }
        y += btnH + 8;

        GUI.Label(new Rect(gsX, y, 170, 20), $"Sync: {_syncInterval * 1000:F0}ms");
        y += 18;
        _syncInterval = Mathf.Round(GUI.HorizontalSlider(new Rect(gsX, y, btnW, 20), _syncInterval, 0.02f, 0.5f) * 100f) / 100f;

        // ── Log Area ──

        var logX = 380f;
        var logW = Screen.width - logX - 10f;
        var logH = Screen.height - 50f;

        GUI.Label(new Rect(logX, 5, logW, 20), "═══ Log ═══");

        if (GUI.Button(new Rect(Screen.width - 90, 5, 40, 20), "Clear"))
        {
            _logLines.Clear();
        }
        _autoScroll = GUI.Toggle(new Rect(Screen.width - 130, 5, 40, 20), _autoScroll, "Auto");

        _scrollPos = GUI.BeginScrollView(
            new Rect(logX, 28, logW, logH),
            _scrollPos,
            new Rect(0, 0, logW - 25, _logLines.Count * 20));

        for (int i = 0; i < _logLines.Count; i++)
        {
            GUI.Label(new Rect(5, i * 20, logW - 25, 20), _logLines[i]);
        }

        if (_autoScroll && Event.current.type == EventType.Repaint)
        {
            _scrollPos.y = Mathf.Max(0, _logLines.Count * 20 - logH);
        }

        GUI.EndScrollView();
    }

    struct EntitySyncTarget
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    void ApplyAnimSync(GameObject go, string animName, float animTime)
    {
        if (string.IsNullOrEmpty(animName))
            return;
        var animator = go.GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        int hash = int.Parse(animName);

        var state = animator.GetCurrentAnimatorStateInfo(0);
        bool sameAnim = state.fullPathHash == hash;
        if (sameAnim)
            return;

        //animator.Play(hash);
    }

    void ApplyAnimParams(GameObject go, Dictionary<string, int> intParams, Dictionary<string, float> floatParams, Dictionary<string, bool> boolParams)
    {
        var animator = go.GetComponent<Animator>();
        if (animator == null)
            return;

        foreach (var kv in intParams)
            animator.SetInteger(kv.Key, kv.Value);
        foreach (var kv in floatParams)
            animator.SetFloat(kv.Key, kv.Value);
        foreach (var kv in boolParams)
            animator.SetBool(kv.Key, kv.Value);
    }
}
