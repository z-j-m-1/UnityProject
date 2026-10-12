using System;
using UnityEngine;
using XNode;

/// <summary>
/// 子图参数输入-曲线（取值源）：**不走变量系统**。
/// AnimationCurve 不在 <c>VariableBundle</c> 支持的类型里 —— 若沿用通用基类
/// <c>SubGraphInputNode&lt;T&gt;</c> 的"未注入时回退图变量"，会抛 <c>NotSupportedException</c>
/// （严重到连编辑器**悬停画端口提示**都会崩）。所以照「参数/输入/物体」写成独立节点。
///
/// 父图的 SubGraphNode 会**自动**生成同名的 AnimationCurve 输入端口（端口类型取自本节点的 ParamType），
/// 所以 SubGraphNode 本身不需要任何改动；执行前父图把连线值注入"统一调用参数存储"，
/// 子图内部连本节点输出端口取这条曲线。
/// 未注入时返回 null —— 插值节点那边的曲线求值会自动退回**线性**（不会报错）。
/// </summary>
[CreateNodeMenu("参数/输入/曲线")]
public class SubGraphInputCurveNode : SubGraphInputNodeBase
{
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public AnimationCurve value;

    public override Type ParamType => typeof(AnimationCurve);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
        {
            // 统一调用参数优先；未注入时回退节点字段默认值（null = 线性）
            if (graph is BaseNodeGraph g && !string.IsNullOrEmpty(parameterName)
                && g.TryGetInvocationParam(parameterName, out object v) && v is AnimationCurve curve)
            {
                value = curve;
                return curve;
            }
            return value;
        }
        return null;
    }
}
