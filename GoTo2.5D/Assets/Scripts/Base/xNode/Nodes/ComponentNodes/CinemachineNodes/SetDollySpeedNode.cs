using Cinemachine;
using UnityEngine;
using XNode;

/// <summary>相机-轨道速度：CinemachineDollyCart 沿路径移动速度（过场用）。</summary>
[CreateNodeMenu("相机/轨道速度")]
[NodeTint("#FFAA44")]
public class SetDollySpeedNode : ComponentActionNode<CinemachineDollyCart>
{
    [Header("轨道速度")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float speed;

    protected override void Apply(CinemachineDollyCart cart)
    {
        cart.m_Speed = GetInputValue<float>(nameof(speed), speed);
    }
}