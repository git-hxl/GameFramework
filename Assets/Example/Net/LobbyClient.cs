using System;
using LiteNetLib;
using LiteNetLib.Utils;
using MessagePack;
using SharedLib.Models;
using SharedLib.Protocol;

public class LobbyClient
{
    public event Action<string> OnLog;

    public bool IsConnected => _peer != null;

    public int Ping => _peer?.Ping ?? 0;
    public int Mtu => _peer?.Mtu ?? 0;

    const string ConnectionKey = "Game@wasd9527";

    EventBasedNetListener _listener;
    NetManager _client;
    NetPeer _peer;

    public LobbyClient()
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
            Log("[Lobby] Connected");
        };

        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
            _peer = null;
            Log($"[Lobby] Disconnected: {info.Reason}");
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
            Log("[Lobby] Already connected");
            return;
        }
        _client.Connect(address, port, ConnectionKey);
        Log($"[Lobby] Connecting to {address}:{port} ...");
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
            Log("[Lobby] Not connected");
            return;
        }
        var writer = new NetDataWriter();
        writer.Put(msgId);
        writer.Put((byte)0);
        writer.Put(payload);
        _peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void SendJoinLobby(PlayerInfo player)
    {
        var req = new JoinLobbyRequest { Player = player };
        Send(MessageIds.JoinLobby, MessagePackSerializer.Serialize(req));
        Log($"[Lobby] -> JoinLobby player={player.Nickname}");
    }

    public void SendLeaveLobby(long userId)
    {
        var req = new LeaveLobbyRequest { UserId = userId };
        Send(MessageIds.LeaveLobby, MessagePackSerializer.Serialize(req));
        Log($"[Lobby] -> LeaveLobby userId={userId}");
    }

    public void SendChat(long userId, string nickname, string content)
    {
        var req = new ChatRequest
        {
            UserId = userId,
            Nickname = nickname,
            Content = content
        };
        Send(MessageIds.Chat, MessagePackSerializer.Serialize(req));
        Log($"[Chat] -> {content}");
    }

    public void SendCreateRoom(string roomId)
    {
        var req = new CreateRoomRequest { RoomId = roomId, RoomType = RoomType.Default };
        Send(MessageIds.CreateRoom, MessagePackSerializer.Serialize(req));
        Log($"[Lobby] -> CreateRoom roomId={roomId}");
    }

    public void SendJoinRoom(string roomId)
    {
        var req = new JoinRoomRequest { RoomId = roomId };
        Send(MessageIds.JoinRoom, MessagePackSerializer.Serialize(req));
        Log($"[Lobby] -> JoinRoom roomId={roomId}");
    }

    public void SendLeaveRoom()
    {
        var req = new LeaveRoomRequest();
        Send(MessageIds.LeaveRoom, MessagePackSerializer.Serialize(req));
        Log("[Lobby] -> LeaveRoom");
    }

    public void SendRoomList()
    {
        var req = new RoomListRequest();
        Send(MessageIds.RoomList, MessagePackSerializer.Serialize(req));
        Log("[Lobby] -> RoomList");
    }

    public void SendGameReady()
    {
        var req = new GameReadyRequest();
        Send(MessageIds.GameReady, MessagePackSerializer.Serialize(req));
        Log("[Lobby] -> GameReady");
    }

    public void SendGameUnready()
    {
        var req = new GameReadyRequest();
        Send(MessageIds.GameUnready, MessagePackSerializer.Serialize(req));
        Log("[Lobby] -> GameUnready");
    }

    public void SendGameStart()
    {
        var req = new GameStartRequest();
        Send(MessageIds.GameStart, MessagePackSerializer.Serialize(req));
        Log("[Lobby] -> GameStart");
    }

    // ── Receive ──────────────────────────────

    void HandleMessage(ushort msgId, byte code, byte[] payload)
    {
        try
        {
            var rc = (ReturnCode)code;
            switch (msgId)
            {
                case MessageIds.JoinLobby:
                    if (rc == ReturnCode.Success)
                    {
                        var resp = MessagePackSerializer.Deserialize<JoinLobbyResponse>(payload);
                        Log($"[Lobby] JoinLobbyResponse OK, player={resp.Player.Nickname}");
                    }
                    else Log($"[Lobby] JoinLobbyResponse error: {rc}");
                    break;

                case MessageIds.LeaveLobby:
                    if (rc == ReturnCode.Success)
                    {
                        var resp = MessagePackSerializer.Deserialize<LeaveLobbyResponse>(payload);
                        Log($"[Lobby] LeaveLobbyResponse OK, userId={resp.UserId}");
                    }
                    else Log($"[Lobby] LeaveLobbyResponse error: {rc}");
                    break;

                case MessageIds.Chat:
                case MessageIds.ChatNotify:
                    var chat = MessagePackSerializer.Deserialize<ChatNotify>(payload);
                    Log($"[Chat] {chat.Nickname}: {chat.Content}");
                    break;

                case MessageIds.CreateRoom:
                    if (rc == ReturnCode.Success)
                    {
                        var resp = MessagePackSerializer.Deserialize<CreateRoomResponse>(payload);
                        Log($"[Lobby] CreateRoomResponse OK, room={resp.Room.RoomId}, " +
                            $"gs={resp.Room.GameServerAddress}:{resp.Room.GameServerPort}, " +
                            $"players={resp.Room.Players?.Count ?? 0}");
                    }
                    else Log($"[Lobby] CreateRoomResponse error: {rc}");
                    break;

                case MessageIds.JoinRoom:
                    if (rc == ReturnCode.Success)
                    {
                        var resp = MessagePackSerializer.Deserialize<JoinRoomResponse>(payload);
                        Log($"[Lobby] JoinRoomResponse OK, room={resp.Room.RoomId}, players={resp.Room.Players?.Count ?? 0}");
                    }
                    else Log($"[Lobby] JoinRoomResponse error: {rc}");
                    break;

                case MessageIds.JoinRoomNotify:
                    var joinNotify = MessagePackSerializer.Deserialize<JoinRoomNotify>(payload);
                    Log($"[Lobby] JoinRoomNotify: {joinNotify.Player.Nickname} -> room {joinNotify.RoomId}");
                    break;

                case MessageIds.LeaveRoom:
                    if (rc == ReturnCode.Success)
                    {
                        var resp = MessagePackSerializer.Deserialize<LeaveRoomResponse>(payload);
                        Log($"[Lobby] LeaveRoomResponse OK, room={resp.RoomId}");
                    }
                    else Log($"[Lobby] LeaveRoomResponse error: {rc}");
                    break;

                case MessageIds.LeaveRoomNotify:
                    var leaveRoomNotify = MessagePackSerializer.Deserialize<LeaveRoomNotify>(payload);
                    Log($"[Lobby] LeaveRoomNotify: userId={leaveRoomNotify.UserId} left room {leaveRoomNotify.RoomId}");
                    break;

                case MessageIds.RoomList:
                    if (rc == ReturnCode.Success)
                    {
                        var resp = MessagePackSerializer.Deserialize<RoomListResponse>(payload);
                        var cnt = resp.Rooms?.Count ?? 0;
                        Log($"[Lobby] RoomListResponse OK, {cnt} rooms");
                        if (resp.Rooms != null)
                            foreach (var r in resp.Rooms)
                                Log($"  room={r.RoomId}, type={r.RoomType}, players={r.PlayerCount}");
                    }
                    else Log($"[Lobby] RoomListResponse error: {rc}");
                    break;

                case MessageIds.GameReady:
                    if (rc == ReturnCode.Success)
                    {
                        var ready = MessagePackSerializer.Deserialize<GameReadyResponse>(payload);
                        Log($"[Lobby] GameReadyResponse: {ready.ReadyCount}/{ready.TotalCount}, allReady={ready.AllReady}");
                    }
                    else Log($"[Lobby] GameReadyResponse error: {rc}");
                    break;

                case MessageIds.GameUnready:
                    if (rc == ReturnCode.Success)
                    {
                        var unready = MessagePackSerializer.Deserialize<GameUnreadyResponse>(payload);
                        Log($"[Lobby] GameUnreadyResponse: {unready.ReadyCount}/{unready.TotalCount}");
                    }
                    else Log($"[Lobby] GameUnreadyResponse error: {rc}");
                    break;

                case MessageIds.GameStart:
                    if (rc == ReturnCode.Success)
                    {
                        var start = MessagePackSerializer.Deserialize<GameStartResponse>(payload);
                        Log($"[Lobby] GameStartResponse: code={start.Code}");
                    }
                    else Log($"[Lobby] GameStartResponse error: {rc}");
                    break;

                case MessageIds.GameStartNotify:
                    var notify = MessagePackSerializer.Deserialize<GameStartNotify>(payload);
                    Log($"[Lobby] GameStartNotify: room={notify.RoomId}, gs={notify.GameServerAddress}:{notify.GameServerPort}");
                    OnGameServerAssigned?.Invoke(notify.GameServerAddress, notify.GameServerPort);
                    break;

                default:
                    Log($"[Lobby] Unknown msgId={msgId}, code={rc}, len={payload.Length}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log($"[Lobby] Deserialize error msgId={msgId}: {ex.Message}");
        }
    }

    // ── Events ───────────────────────────────

    public event Action<string, int> OnGameServerAssigned;

    void Log(string msg)
    {
        OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");
    }
}
