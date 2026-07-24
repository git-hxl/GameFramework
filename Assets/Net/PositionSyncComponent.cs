using GameFramework;
using SharedLib.Models;
using UnityEngine;

public class PositionSyncComponent : MonoBehaviour
{
    public long EntityId;
    public bool IsLocal;
    public float SyncInterval = 0.1f;

    float _timer;
    Vector3 _lastPos;
    Quaternion _lastRot;

    Vector3 _targetPos;
    Quaternion _targetRot;
    bool _hasTarget;
    GameClient _client;

    void Start()
    {
        _client = NetExample.GameClientInstance;
        if (_client == null) return;

        if (!IsLocal)
            _client.OnPositionSyncReceived += OnReceive;
    }

    void OnDestroy()
    {
        if (_client != null && !IsLocal)
            _client.OnPositionSyncReceived -= OnReceive;
    }

    void Update()
    {
        if (_client == null) return;

        if (IsLocal)
            UpdateSend();
        else if (_hasTarget)
            UpdateInterpolate();
    }

    void UpdateSend()
    {
        _timer += Time.deltaTime;
        if (_timer < SyncInterval) return;
        _timer = 0f;

        var t = transform;
        var pos = t.position;
        var rot = t.rotation;

        if (Vector3.Distance(pos, _lastPos) < 0.001f && Quaternion.Angle(rot, _lastRot) < 0.1f)
            return;

        _lastPos = pos;
        _lastRot = rot;

        var angles = rot.eulerAngles;
        _client.SendPositionSync(new PositionSyncData
        {
            EntityId = EntityId,
            EntityType = 0,
            PosX = pos.x,
            PosY = pos.y,
            PosZ = pos.z,
            RotX = angles.x,
            RotY = angles.y,
            RotZ = angles.z
        });
    }

    void OnReceive(PositionSyncData data)
    {
        if (data.EntityId != EntityId) return;

        _targetPos = new Vector3(data.PosX, data.PosY, data.PosZ);
        _targetRot = Quaternion.Euler(data.RotX, data.RotY, data.RotZ);
        _hasTarget = true;
    }

    void UpdateInterpolate()
    {
        float dt = Time.deltaTime;
        float maxSpeed = Vector3.Distance(transform.position, _targetPos) / SyncInterval;
        transform.position = Vector3.MoveTowards(transform.position, _targetPos, maxSpeed * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRot, Mathf.Clamp01(dt / SyncInterval * 2f));
    }
}
