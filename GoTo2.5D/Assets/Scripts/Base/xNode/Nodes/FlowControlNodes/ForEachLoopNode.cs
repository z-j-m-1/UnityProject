using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 流程-遍历列表（泛型基类）：逐元素执行循环体（body 子链，以 EndNode 收尾），
/// 每轮把当前元素写到 item 输出端口；遍历完沿 next 继续。
/// 注意：遍历过程中不要修改列表（会导致跳过元素）。
/// 具体类型子类各自成同名单文件。
/// </summary>
public abstract class ForEachLoopNode<T> : FlowNode
{
    [Header("列表（接「获取列表变量」/「参数/输入」）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public List<T> list;

    [Header("当前元素（每轮更新）")]
    [Output(ShowBackingValue.Always)]
    public T item;

    [Header("循环体（每轮执行，用 EndNode 收尾）")]
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public BaseNode body;

    private MonoBehaviour loopHost;

    public override void Execute()
    {
        loopHost = NodeExecuteContext.Current;
        base.Execute();
    }

    public override IEnumerator GetFlow()
    {
        List<T> target = GetInputValue<List<T>>(nameof(list), list);
        if (target == null)
        {
            NodeLog.Warning($"{GetType().Name}: 列表为空（list 未接线且节点上无默认列表），跳过循环");
            yield break;
        }

        BaseNode bodyStart = GetBodyStart();
        if (bodyStart == null)
        {
            NodeLog.Warning($"{GetType().Name}: 循环体未连接");
            yield break;
        }

        foreach (T element in target)
        {
            item = element;
            yield return GraphChainRunner.RunChain(graph as BaseNodeGraph, bodyStart, loopHost, null);
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(list))
            return GetInputValue<List<T>>(nameof(list), list);
        if (port.fieldName == nameof(item))
            return item;
        return null;
    }

    private BaseNode GetBodyStart()
    {
        NodePort port = GetOutputPort(nameof(body));
        if (port != null && port.IsConnected)
        {
            NodePort connection = port.GetConnection(0);
            if (connection != null) return connection.node as BaseNode;
        }
        return null;
    }
}