using UnityEngine;
using XNode;

/// <summary>刚体-施加力（2D，世界空间牛顿力；目标解析沿用 ComponentActionNode 的输入端口 / 目标模式）</summary>
[CreateNodeMenu("刚体/施加力(2D)")]
[NodeTint("#FF8844")]
public class Rigidbody2DAddForceNode : ComponentActionNode<Rigidbody2D>
{
    [Header("力（世界空间牛顿）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector2 force;

    [Header("施加方式")]
    public ForceMode2D forceMode = ForceMode2D.Force;

    protected override void Apply(Rigidbody2D rb)
    {
        rb.AddForce(GetInputValue<Vector2>(nameof(force), force), forceMode);
    }
}