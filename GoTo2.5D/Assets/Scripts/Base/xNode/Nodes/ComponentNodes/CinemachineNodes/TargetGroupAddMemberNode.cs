using Cinemachine;
using UnityEngine;
using XNode;

/// <summary>相机-目标组添加成员：向 CinemachineTargetGroup 添加成员（多目标取景）。</summary>
[CreateNodeMenu("相机/目标组添加成员")]
[NodeTint("#FFAA44")]
public class TargetGroupAddMemberNode : ComponentActionNode<CinemachineTargetGroup>
{
    [Header("成员物体（可接 获取物体 / 参数输入/物体）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject member;

    [Header("权重")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float weight = 1f;

    [Header("半径")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float radius = 0f;

    protected override void Apply(CinemachineTargetGroup group)
    {
        GameObject m = GetInputValue<GameObject>(nameof(member), null);
        if (m == null)
        {
            NodeLog.Warning($"{GetType().Name}: 成员物体为空");
            return;
        }
        group.AddMember(m.transform, GetInputValue<float>(nameof(weight), weight), GetInputValue<float>(nameof(radius), radius));
    }
}