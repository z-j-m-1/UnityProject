using UnityEngine;
using XNode;

/// <summary>
/// 值-曲线：输出一条**可在节点面板上直接编辑**的 <see cref="AnimationCurve"/>。
/// 用途：
///   · 接到插值节点（<c>TweenNodeBase</c> 派生）的「曲线输入」端口 → 由外部决定缓动；
///   · 接到父图 SubGraphNode 的曲线输入端口 → 把缓动曲线传进子图；
///   · 接到「参数/输出/曲线」→ 把曲线作为子图的返回值。
///
/// 实现刻意不用 <c>ValueNode&lt;AnimationCurve&gt;</c>：那个基类的输出端口是
/// <c>ShowBackingValue.Always</c>（字段画在端口旁边），而曲线编辑器很高、塞进端口那一列会被压扁。
/// 这里把曲线做成**整宽普通字段**，输出端口单独用非序列化端口（由默认节点体编辑器自动补画）。
/// </summary>
[CreateNodeMenu("值/曲线")]
[NodeWidth(300)]
public class CurveValueNode : DataNode
{
    [Header("曲线（横轴 0~1 进度，纵轴 0~1 混合系数）")]
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("输出")]
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public AnimationCurve output;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(output))
        {
            output = curve;
            return output;
        }
        return null;
    }
}
