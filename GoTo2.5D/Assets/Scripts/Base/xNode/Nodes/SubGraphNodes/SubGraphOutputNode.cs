using System;
using UnityEngine;
using XNode;

/// <summary>子图参数输出节点（非泛型基类）：声明子图的一个输出参数，参数名 = 子图变量名</summary>
public abstract class SubGraphOutputNodeBase : DataNode
{
    [Header("参数名（图中唯一，= 子图变量名）")]
    public string parameterName;

    /// <summary>参数类型（泛型子类提供）</summary>
    public abstract Type ParamType { get; }

    /// <summary>求值输出参数值（子图链跑完后由 SubGraphNode 调用；未连线返回节点字段默认值）</summary>
    public abstract object EvaluateValue();
}

/// <summary>
/// 子图参数输出节点（返回值槽）：父图 SubGraphNode 会自动生成匹配的输出端口（端口名 = 参数名），
/// 子图内部把结果连到本节点的「输入」端口；子图链跑完后父图求值读回并暴露到输出端口。
/// </summary>
public abstract class SubGraphOutputNode<T> : SubGraphOutputNodeBase
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T value;

    public override Type ParamType => typeof(T);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
        {
            return EvaluateValue();
        }
        return null;
    }

    public override object EvaluateValue()
    {
        return GetInputValue<T>(nameof(value), value);
    }
}
