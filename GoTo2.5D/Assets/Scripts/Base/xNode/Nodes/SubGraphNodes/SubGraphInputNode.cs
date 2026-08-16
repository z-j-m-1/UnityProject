using System;
using UnityEngine;
using XNode;

/// <summary>子图参数输入节点（非泛型基类）：声明子图的一个输入参数，参数名 = 子图变量名</summary>
public abstract class SubGraphInputNodeBase : DataNode
{
    [Header("参数名（图中唯一，= 子图变量名）")]
    public string parameterName;

    /// <summary>参数类型（泛型子类提供）</summary>
    public abstract Type ParamType { get; }
}

/// <summary>
/// 子图参数输入节点：父图 SubGraphNode 会自动生成匹配的输入端口（端口名 = 参数名），
/// 执行时把父图连线值写入子图变量；子图内部用「获取变量」或本节点读取。
/// </summary>
public abstract class SubGraphInputNode<T> : SubGraphInputNodeBase
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T value;

    public override Type ParamType => typeof(T);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
        {
            if (graph is BaseNodeGraph g && !string.IsNullOrEmpty(parameterName))
            {
                // 父图注入后即子图变量当前值；未注入时返回节点默认值
                return g.Get(parameterName, value);
            }
            return value;
        }
        return null;
    }
}
