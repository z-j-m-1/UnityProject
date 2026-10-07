using UnityEngine;
using XNode;

/// <summary>刚体操作模式（3D）</summary>
public enum RigidbodyOpMode
{
    /// <summary>施加力（AddForce，受 forceMode 影响）</summary>
    AddForce,
    /// <summary>设置速度（velocity 直接赋值）</summary>
    SetVelocity,
    /// <summary>设置角速度（angularVelocity 直接赋值）</summary>
    SetAngularVelocity
}

/// <summary>
/// 刚体-力·速度·角速度：**一个节点覆盖原来的三个**
/// （「刚体/施加力」RigidbodyAddForceNode / 「刚体/设置速度」RigidbodySetVelocityNode /
/// 「刚体/设置角速度」RigidbodySetAngularVelocityNode，三者只差一个 Vector3 与写入的属性）。
///
/// ⚠️ 3D 与 2D **不能**合并成一个节点：2D 用的是 <c>Rigidbody2D</c> + <c>Vector2</c> 端口，
/// 端口类型不同 —— 而端口类型在 xNode 里是静态的，不能靠枚举切换（见「刚体/力·速度(2D)」）。
/// </summary>
[CreateNodeMenu("刚体/力·速度·角速度")]
[NodeTint("#FF8844")]
public class RigidbodyOpNode : ComponentActionNode<Rigidbody>
{
    [Header("操作")]
    public RigidbodyOpMode mode = RigidbodyOpMode.AddForce;

    [Header("力 / 速度 / 角速度（世界空间）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 value;

    [Header("施加方式（仅 施加力 使用）")]
    public ForceMode forceMode = ForceMode.Force;

    protected override void Apply(Rigidbody rb)
    {
        Vector3 v = GetInputValue<Vector3>(nameof(value), value);

        switch (mode)
        {
            case RigidbodyOpMode.SetVelocity:
                rb.velocity = v;
                break;

            case RigidbodyOpMode.SetAngularVelocity:
                rb.angularVelocity = v;
                break;

            default:
                rb.AddForce(v, forceMode);
                break;
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
            return GetInputValue<Vector3>(nameof(value), value);
        return null;
    }
}
