using UnityEngine;
using XNode;

/// <summary>变换-移动物体节点（对目标 Transform 增加位移偏移）</summary>
[CreateNodeMenu("变换/移动")]
[NodeTint("#44AAFF")]
public class MoveObjectNode : ComponentActionNode<Transform>
{
    [Header("移动偏移量")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 moveOffset;

    protected override void Apply(Transform t)
    {
        t.position += GetInputValue<Vector3>(nameof(moveOffset), moveOffset);
    }
}
