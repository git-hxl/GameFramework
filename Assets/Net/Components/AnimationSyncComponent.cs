using SharedLib.Models;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationSyncComponent : MonoBehaviour
{
    public long EntityId;
    public bool IsLocal;
    public float SyncInterval = 0.1f;

    float _timer;
    Animator _animator;

    int _lastAnimHash;
    System.Collections.Generic.Dictionary<string, int> _lastIntParams = new();
    System.Collections.Generic.Dictionary<string, float> _lastFloatParams = new();
    System.Collections.Generic.Dictionary<string, bool> _lastBoolParams = new();

    GameClient _client;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _client = NetExample.GameClientInstance;
        if (_client == null) return;

        if (!IsLocal)
            _client.OnAnimationSyncReceived += OnReceive;
    }

    void OnDestroy()
    {
        if (_client != null && !IsLocal)
            _client.OnAnimationSyncReceived -= OnReceive;
    }

    void Update()
    {
        if (_client == null || _animator == null) return;

        if (IsLocal)
        {
            _timer += Time.deltaTime;
            if (_timer < SyncInterval) return;
            _timer = 0f;
            SendSync();
        }
    }

    void SendSync()
    {
        var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        int hash = stateInfo.fullPathHash;
        float time = stateInfo.normalizedTime;

        if (hash == _lastAnimHash && !HasParamChanged())
            return;

        _lastAnimHash = hash;

        var data = new AnimationSyncData
        {
            EntityId = EntityId,
            AnimName = hash.ToString(),
            AnimNormalTime = time,
            IntParams = new System.Collections.Generic.Dictionary<string, int>(),
            FloatParams = new System.Collections.Generic.Dictionary<string, float>(),
            BoolParams = new System.Collections.Generic.Dictionary<string, bool>()
        };

        foreach (var p in _animator.parameters)
        {
            switch (p.type)
            {
                case AnimatorControllerParameterType.Int:
                    var iv = _animator.GetInteger(p.nameHash);
                    _lastIntParams[p.name] = iv;
                    data.IntParams[p.name] = iv;
                    break;
                case AnimatorControllerParameterType.Float:
                    var fv = _animator.GetFloat(p.nameHash);
                    _lastFloatParams[p.name] = fv;
                    data.FloatParams[p.name] = fv;
                    break;
                case AnimatorControllerParameterType.Bool:
                    var bv = _animator.GetBool(p.nameHash);
                    _lastBoolParams[p.name] = bv;
                    data.BoolParams[p.name] = bv;
                    break;
            }
        }

        _client.SendAnimationSync(data);
    }

    bool HasParamChanged()
    {
        foreach (var p in _animator.parameters)
        {
            switch (p.type)
            {
                case AnimatorControllerParameterType.Int:
                    var iv = _animator.GetInteger(p.nameHash);
                    if (!_lastIntParams.TryGetValue(p.name, out var lastIv) || lastIv != iv) return true;
                    break;
                case AnimatorControllerParameterType.Float:
                    var fv = _animator.GetFloat(p.nameHash);
                    if (!_lastFloatParams.TryGetValue(p.name, out var lastFv) || Mathf.Abs(lastFv - fv) > 0.001f) return true;
                    break;
                case AnimatorControllerParameterType.Bool:
                    var bv = _animator.GetBool(p.nameHash);
                    if (!_lastBoolParams.TryGetValue(p.name, out var lastBv) || lastBv != bv) return true;
                    break;
            }
        }
        return false;
    }

    void OnReceive(AnimationSyncData data)
    {
        if (data.EntityId != EntityId) return;

        ApplyAnimParams(data.IntParams, data.FloatParams, data.BoolParams);
    }

    void ApplyAnimParams(System.Collections.Generic.Dictionary<string, int> intParams, System.Collections.Generic.Dictionary<string, float> floatParams, System.Collections.Generic.Dictionary<string, bool> boolParams)
    {
        foreach (var kv in intParams) _animator.SetInteger(kv.Key, kv.Value);
        foreach (var kv in floatParams) _animator.SetFloat(kv.Key, kv.Value);
        foreach (var kv in boolParams) _animator.SetBool(kv.Key, kv.Value);
    }
}
