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
}

/// <summary>
/// 子图参数输出节点：父图 SubGraphNode 会自动生成匹配的输出端口（端口名 = 参数名），
/// 子图内部用「设置变量」写入同名变量；子图链跑完后父图读回并暴露到输出端口。
/// </summary>
public abstract class SubGraphOutputNode<T> : SubGraphOutputNodeBase
{
    [Output]
    public T value;

    public override Type ParamType => typeof(T);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
        {
            if (graph is BaseNodeGraph g && !string.IsNullOrEmpty(parameterName))
            {
                return g.Get(parameterName, value);
            }
            return value;
        }
        return null;
    }
}

[CreateNodeMenu("子图/参数输出/字符串")]
public class SubGraphOutputStringNode : SubGraphOutputNode<string>
{
}

[CreateNodeMenu("子图/参数输出/布尔")]
public class SubGraphOutputBoolNode : SubGraphOutputNode<bool>
{
}

[CreateNodeMenu("子图/参数输出/整数")]
public class SubGraphOutputIntNode : SubGraphOutputNode<int>
{
}

[CreateNodeMenu("子图/参数输出/浮点")]
public class SubGraphOutputFloatNode : SubGraphOutputNode<float>
{
}

[CreateNodeMenu("子图/参数输出/三维向量")]
public class SubGraphOutputVector3Node : SubGraphOutputNode<Vector3>
{
}
