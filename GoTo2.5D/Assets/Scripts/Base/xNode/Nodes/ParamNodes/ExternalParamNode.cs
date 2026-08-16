using UnityEngine;
using XNode;

/// <summary>
/// 外部参数读取节点的非泛型基类（供自定义编辑器按类型定位 / paramName 下拉收集）
/// </summary>
public abstract class ExternalParamNodeBase : DataNode
{
    [Header("参数名（与外部触发时传入的键一致）")]
    public string paramName;
}

/// <summary>
/// 外部参数读取节点基类（取值源）：
/// 外部 C# 代码在触发图时用 GraphParams 携带命名参数（GraphExecutor.ExecuteFromEntry / GraphEvent.data），
/// 本节点按 paramName 读取输出。参数是瞬态图级存储（不序列化、不进存档）：
/// 最近一次带参触发注入的那批，直到下次带参触发被替换。
/// 具体类型子类各自成同名单文件。
/// </summary>
public abstract class ExternalParamNode<T> : ExternalParamNodeBase
{
    [Header("默认值（参数缺失/类型不符时返回；物体类型请留空）")]
    public T fallback;

    [Output]
    [System.NonSerialized]
    public T value;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
        {
            if (graph is BaseNodeGraph g)
            {
                value = g.GetExternalParam(paramName, fallback);
                return value;
            }
            return fallback;
        }
        return null;
    }
}