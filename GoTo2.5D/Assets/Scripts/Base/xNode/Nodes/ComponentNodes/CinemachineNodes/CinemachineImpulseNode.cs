using Cinemachine;
using UnityEngine;
using XNode;

/// <summary>
/// 相机-震屏：给 CinemachineImpulseSource 施加冲击（命中/爆炸等）。
/// 位置口接上 → GenerateImpulseAt(pos, vel)；速度口接上 → GenerateImpulse(vel)；都未接 → GenerateImpulse()。
/// 相机需有 CinemachineImpulseListener 才可见震动。
/// </summary>
[CreateNodeMenu("相机/震屏")]
[NodeTint("#FFAA44")]
public class CinemachineImpulseNode : ComponentActionNode<CinemachineImpulseSource>
{
    [Header("震源位置（世界空间，可选）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 position;

    [Header("冲击速度（可选）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 velocity;

    protected override void Apply(CinemachineImpulseSource source)
    {
        bool posConnected = GetPort(nameof(position)).IsConnected;
        bool velConnected = GetPort(nameof(velocity)).IsConnected;
        Vector3 pos = GetInputValue<Vector3>(nameof(position), position);
        Vector3 vel = GetInputValue<Vector3>(nameof(velocity), velocity);

        if (posConnected)
        {
            source.GenerateImpulseAt(pos, vel);
        }
        else if (velConnected)
        {
            source.GenerateImpulse(vel);
        }
        else
        {
            source.GenerateImpulse();
        }
    }
}