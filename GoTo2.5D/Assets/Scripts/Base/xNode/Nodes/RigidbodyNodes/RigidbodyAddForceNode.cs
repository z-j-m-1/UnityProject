using UnityEngine;
using XNode;

/// <summary>刚体-施加力（3D，世界空间牛顿力；目标解析沿用 ComponentActionNode 的输入端口 / 目标模式）</summary>
[CreateNodeMenu("刚体/施加力")]
[NodeTint("#FF8844")]
public class RigidbodyAddForceNode : ComponentActionNode<Rigidbody>
{
    [Header("力（世界空间牛顿）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 force;

    [Header("施加方式")]
    public ForceMode forceMode = ForceMode.Force;

    protected override void Apply(Rigidbody rb)
    {
        rb.AddForce(GetInputValue<Vector3>(nameof(force), force), forceMode);
    }
}