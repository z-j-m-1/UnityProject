using UnityEngine;
using XNode;

/// <summary>变换操作模式</summary>
public enum TransformOperation
{
    /// <summary>设置位置（**绝对**，世界空间）</summary>
    SetPosition,
    /// <summary>设置旋转（**绝对**，世界空间，欧拉角）</summary>
    SetRotation,
    /// <summary>移动（**相对偏移**，世界空间：position += 值）</summary>
    MoveBy,
    /// <summary>旋转（**相对偏移**，世界空间：rotation *= 欧拉角(值)）</summary>
    RotateBy,
    /// <summary>缩放（**相对偏移**，本地空间：localScale += 值）</summary>
    ScaleBy
}

/// <summary>
/// 变换-位置·旋转·缩放：**一个节点覆盖原来的五个**
/// （设置位置 SetPositionNode / 设置旋转 SetRotationNode / 移动 MoveObjectNode / 旋转 RoteObjectNode / 缩放 ScaleObjectNode，
/// 它们各自都只有一个 Vector3 字段，属于典型的"只差模式"同构节点）。
///
/// 各模式的行为**与原节点逐行一致**：
///   SetPosition → t.position = 值
///   SetRotation → t.rotation = Quaternion.Euler(值)
///   MoveBy      → t.position += 值
///   RotateBy    → t.rotation *= Quaternion.Euler(值)
///   ScaleBy     → t.localScale += 值
///
/// ⚠️ 注意 值 字段的默认值是 <c>Vector3.zero</c>：原 ScaleObjectNode 的默认值是 <c>Vector3.one</c>，
/// 但它的代码是<b>加算</b>（localScale += 值），所以那个默认值会让"不改参数直接执行"每次都把物体放大 1 —— 属于笔误，这里已改为 zero。
/// </summary>
[CreateNodeMenu("变换/位置·旋转·缩放")]
[NodeTint("#44AAFF")]
[NodeWidth(300)]
public class TransformOpNode : ComponentActionNode<Transform>
{
    [Header("操作")]
    public TransformOperation operation = TransformOperation.SetPosition;

    [Header("值（设置模式=绝对值；移动/旋转/缩放=偏移量；旋转相关为欧拉角）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 value;

    protected override void Apply(Transform t)
    {
        Vector3 v = GetInputValue<Vector3>(nameof(value), value);

        switch (operation)
        {
            case TransformOperation.SetRotation:
                t.rotation = Quaternion.Euler(v);
                break;

            case TransformOperation.MoveBy:
                t.position += v;
                break;

            case TransformOperation.RotateBy:
                t.rotation *= Quaternion.Euler(v);
                break;

            case TransformOperation.ScaleBy:
                t.localScale += v;
                break;

            default:
                t.position = v;
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
