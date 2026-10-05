using UnityEngine;
using XNode;

/// <summary>刚体-设置速度（2D，世界空间；目标解析沿用 ComponentActionNode 的输入端口 / 目标模式）</summary>
[CreateNodeMenu("刚体/设置速度(2D)")]
[NodeTint("#FF8844")]
public class Rigidbody2DSetVelocityNode : ComponentActionNode<Rigidbody2D>
{
    [Header("速度（世界空间）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector2 velocity;

    protected override void Apply(Rigidbody2D rb)
    {
        rb.velocity = GetInputValue<Vector2>(nameof(velocity), velocity);
    }
}