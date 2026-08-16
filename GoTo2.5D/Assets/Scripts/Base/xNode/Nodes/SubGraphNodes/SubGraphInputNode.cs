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
/// 子图参数输入节点（取值源）：父图 SubGraphNode 会自动生成匹配的输入端口（端口名 = 参数名），
/// 执行时把父图连线值写入子图变量；子图内部连本节点的「输出」端口取参数值（未注入时返回节点字段默认值）。
/// </summary>
public abstract class SubGraphInputNode<T> : SubGraphInputNodeBase
{
    [Output(ShowBackingValue.Always)]
    public T value;

    public override Type ParamType => typeof(T);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
        {
            if (graph is BaseNodeGraph g && !string.IsNullOrEmpty(parameterName))
            {
                // 统一调用参数优先（子图节点 / 外部代码 / 事件 / 状态机任一调用方注入）；
                // 未注入时回退图变量 → 节点字段默认值（兼容旧子图资产）
                if (g.TryGetInvocationParam(parameterName, out object v))
                {
                    return v;
                }
                return g.Get(parameterName, value);
            }
            return value;
        }
        return null;
    }
}
