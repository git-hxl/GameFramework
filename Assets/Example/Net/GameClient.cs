using System;
using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using SharedLib.Models;
using SharedLib.Protocol;

public class GameClient
{
    public event Action<string> OnLog;
    public event Action<JoinGameResponse> OnJoinedGame;
    public event Action OnLeftGame;
    public event Action<PlayerInfo> OnPlayerJoinedGame;
    public event Action<PlayerInfo> OnPlayerLeftGame;
    public event Action<ObjectSpawnData> OnObjectSpawnReceived;
    public event Action<ObjectDespawnData> OnObjectDespawnReceived;
    public event Action<PositionSyncData> OnPositionSyncReceived;
    public event Action<AnimationSyncData> OnAnimationSyncReceived;

    public bool IsConnected => _peer != null;

    public int Ping => _peer?.Ping ?? 0;
    public int Mtu => _peer?.Mtu ?? 0;

    const string ConnectionKey = "Game@wasd9527";

    EventBasedNetListener _listener;
    NetManager _client;
    NetPeer _peer;

    public GameClient()
    {
        _listener = new EventBasedNetListener();
        _client = new NetManager(_listener);
    }

    public void Start()
    {
        _client.Start();

        _listener.NetworkReceiveEvent += (peer, reader, channel, deliveryMethod) =>
        {
            var msgId = reader.GetUShort();
            var code = reader.GetByte();
            var payload = reader.GetRemainingBytes();
            HandleMessage(msgId, code, payload);
            reader.Recycle();
        };

        _listener.PeerConnectedEvent += peer =>
        {
            _peer = peer;
            Log("[Game] Connected");
        };

        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
            _peer = null;
            Log($"[Game] Disconnected: {info.Reason}");
            OnLeftGame?.Invoke();
        };
    }

    public void Stop()
    {
        _client?.Stop();
    }

    public void PollEvents()
    {
        _client.PollEvents();
    }

    public void Connect(string address, int port)
    {
        if (_peer != null)
        {
            Log("[Game] Already connected, reconnecting...");
            _peer.Disconnect();
        }
        _client.Connect(address, port, ConnectionKey);
        Log($"[Game] Connecting to {address}:{port} ...");
    }

    public void Disconnect()
    {
        _peer?.Disconnect();
    }

    // ── Send ─────────────────────────────────

    void Send(ushort msgId, byte[] payload)
    {
        if (_peer == null)
        {
            Log("[Game] Not connected");
            return;
        }
        var writer = new NetDataWriter();
        writer.Put(msgId);
        writer.Put((byte)0);
        writer.Put(payload);
        _peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void SendJoinGame(string roomId, PlayerInfo player)
    {
        var req = new JoinGameRequest { RoomId = roomId, Player = player };
        Send(MessageIds.JoinGame, MessagePackSerializer.Serialize(req));
        Log($"[Game] -> JoinGame roomId={roomId}");
    }

    public void SendLeaveGame()
    {
        Send(MessageIds.LeaveGame, Array.Empty<byte>());
        Log("[Game] -> LeaveGame");
    }

    public void SendPositionSync(PositionSyncData data)
    {
        Send(MessageIds.PositionSync, MessagePackSerializer.Serialize(data));
    }

    public void SendAnimationSync(AnimationSyncData data)
    {
        Send(MessageIds.AnimationSync, MessagePackSerializer.Serialize(data));
    }

    public void SendObjectSpawn(ObjectSpawnData data)
    {
        Send(MessageIds.ObjectSpawn, MessagePackSerializer.Serialize(data));
    }

    public void SendObjectDespawn(ObjectDespawnData data)
    {
        Send(MessageIds.ObjectDespawn, MessagePackSerializer.Serialize(data));
    }

    // ── Receive ──────────────────────────────

    void HandleMessage(ushort msgId, byte code, byte[] payload)
    {
        try
        {
            var rc = (ReturnCode)code;
            switch (msgId)
            {
                case MessageIds.JoinGame:
                    if (rc == ReturnCode.Success)
                    {
                        var resp = MessagePackSerializer.Deserialize<JoinGameResponse>(payload);
                        Log($"[Game] JoinGameResponse OK, room={resp.RoomId}, owner={resp.OwnerUserId}, players={resp.Players?.Count ?? 0}");
                        OnJoinedGame?.Invoke(resp);
                    }
                    else Log($"[Game] JoinGameResponse error: {rc}");
                    break;

                case MessageIds.LeaveGame:
                    if (rc == ReturnCode.Success)
                    {
                        Log($"[Game] LeaveGame OK");
                        OnLeftGame?.Invoke();
                    }
                    else
                        Log($"[Game] LeaveGame error: {rc}");
                    break;

                case MessageIds.JoinGameNotify:
                    var joinNotify = MessagePackSerializer.Deserialize<JoinGameNotify>(payload);
                    Log($"[Game] JoinGameNotify: {joinNotify.Player.Nickname} -> room {joinNotify.RoomId}");
                    OnPlayerJoinedGame?.Invoke(joinNotify.Player);
                    break;

                case MessageIds.LeaveGameNotify:
                    var leaveNotify = MessagePackSerializer.Deserialize<LeaveGameNotify>(payload);
                    Log($"[Game] LeaveGameNotify: userId={leaveNotify.UserId} left");
                    OnPlayerLeftGame?.Invoke(new PlayerInfo { UserId = leaveNotify.UserId });
                    break;

                case MessageIds.PositionSync:
                    var posSync = MessagePackSerializer.Deserialize<PositionSyncData>(payload);
                    OnPositionSyncReceived?.Invoke(posSync);
                    break;

                case MessageIds.AnimationSync:
                    var animSync = MessagePackSerializer.Deserialize<AnimationSyncData>(payload);
                    OnAnimationSyncReceived?.Invoke(animSync);
                    break;

                case MessageIds.ObjectSpawn:
                    var spawn = MessagePackSerializer.Deserialize<ObjectSpawnData>(payload);
                    Log($"[Game] ObjectSpawn: id={spawn.ObjectId}, prefab={spawn.PrefabName}, pos=({spawn.PosX:F1},{spawn.PosY:F1},{spawn.PosZ:F1})");
                    OnObjectSpawnReceived?.Invoke(spawn);
                    break;

                case MessageIds.ObjectDespawn:
                    var despawn = MessagePackSerializer.Deserialize<ObjectDespawnData>(payload);
                    Log($"[Game] ObjectDespawn: id={despawn.ObjectId}");
                    OnObjectDespawnReceived?.Invoke(despawn);
                    break;

                case MessageIds.GameStartNotify:
                    var gsNotify = MessagePackSerializer.Deserialize<GameStartNotify>(payload);
                    Log($"[Game] GameStartNotify: room={gsNotify.RoomId}, gs={gsNotify.GameServerAddress}:{gsNotify.GameServerPort}");
                    break;

                default:
                    Log($"[Game] Unknown msgId={msgId}, code={rc}, len={payload.Length}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log($"[Game] Deserialize error msgId={msgId}: {ex.Message}");
        }
    }

    void Log(string msg)
    {
        OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");
    }
}
