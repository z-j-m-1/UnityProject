using System;
using UnityEngine;
using XNode;

/// <summary>
/// 子图参数输出-曲线（返回值槽）：父图 SubGraphNode 会**自动**生成同名输出端口。
/// 子图内部把曲线接到「值」输入端口，子图链跑完后父图读回；**未接线时用节点字段里的默认曲线**。
///
/// 做成独立节点（而不是 <c>SubGraphOutputNode&lt;AnimationCurve&gt;</c>）有两个原因：
///   · 通用基类的 <c>value</c> 是 <c>ShowBackingValue.Unconnected</c> 的输入端口，
///     曲线编辑器塞在端口那一列会被**压扁**；
///   · 顺手把"曲线不经变量系统"这件事写明白（变量系统不支持 AnimationCurve）。
/// </summary>
[CreateNodeMenu("参数/输出/曲线")]
public class SubGraphOutputCurveNode : SubGraphOutputNodeBase
{
    [Header("默认曲线（输入端口未接线时用这个）")]
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("接线：要把子图里的哪条曲线返回出去")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public AnimationCurve source;

    public override Type ParamType => typeof(AnimationCurve);

    public override object EvaluateValue()
    {
        AnimationCurve c = GetInputValue<AnimationCurve>(nameof(source), null);
        return c != null ? c : curve;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(source))
        {
            return GetInputValue<AnimationCurve>(nameof(source), null);
        }
        return null;
    }
}
