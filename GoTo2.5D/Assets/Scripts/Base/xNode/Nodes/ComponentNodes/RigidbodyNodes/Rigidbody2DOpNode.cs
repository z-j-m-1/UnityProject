using UnityEngine;
using XNode;

/// <summary>刚体操作模式（2D）</summary>
public enum Rigidbody2DOpMode
{
    /// <summary>施加力（AddForce，受 forceMode 影响）</summary>
    AddForce,
    /// <summary>设置速度（velocity 直接赋值）</summary>
    SetVelocity
}

/// <summary>
/// 刚体-力·速度(2D)：**一个节点覆盖原来的两个**
/// （「刚体/施加力(2D)」Rigidbody2DAddForceNode / 「刚体/设置速度(2D)」Rigidbody2DSetVelocityNode）。
/// 与 3D 版分成两个节点是刻意的：端口类型 <c>Vector2</c> / <c>Rigidbody2D</c> 与 3D 不同，无法靠枚举切换。
/// </summary>
[CreateNodeMenu("刚体/力·速度(2D)")]
[NodeTint("#FF8844")]
public class Rigidbody2DOpNode : ComponentActionNode<Rigidbody2D>
{
    [Header("操作")]
    public Rigidbody2DOpMode mode = Rigidbody2DOpMode.AddForce;

    [Header("力 / 速度（世界空间）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector2 value;

    [Header("施加方式（仅 施加力 使用）")]
    public ForceMode2D forceMode = ForceMode2D.Force;

    protected override void Apply(Rigidbody2D rb)
    {
        Vector2 v = GetInputValue<Vector2>(nameof(value), value);

        if (mode == Rigidbody2DOpMode.SetVelocity)
        {
            rb.velocity = v;
        }
        else
        {
            rb.AddForce(v, forceMode);
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
            return GetInputValue<Vector2>(nameof(value), value);
        return null;
    }
}
