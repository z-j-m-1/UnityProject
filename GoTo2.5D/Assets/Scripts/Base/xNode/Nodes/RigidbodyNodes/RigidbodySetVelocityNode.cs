using UnityEngine;
using XNode;

/// <summary>刚体-设置速度（3D，世界空间；目标解析沿用 ComponentActionNode 的输入端口 / 目标模式）</summary>
[CreateNodeMenu("刚体/设置速度")]
[NodeTint("#FF8844")]
public class RigidbodySetVelocityNode : ComponentActionNode<Rigidbody>
{
    [Header("速度（世界空间）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 velocity;

    protected override void Apply(Rigidbody rb)
    {
        rb.velocity = GetInputValue<Vector3>(nameof(velocity), velocity);
    }
}