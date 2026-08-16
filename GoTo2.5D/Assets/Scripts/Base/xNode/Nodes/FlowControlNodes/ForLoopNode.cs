using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 流程-计数循环：从 startIndex 开始执行 count 次循环体（body 子链，以 EndNode 收尾），
/// 每轮把当前索引写到 index 输出端口；循环结束后沿 next 继续。
/// 说明：并发跑同一张图时 index 会被互相覆盖（与其他共享节点状态一致）。
/// </summary>
[CreateNodeMenu("流程/计数循环")]
[NodeTint("#44CC88")]
public class ForLoopNode : FlowNode
{
    [Header("起始索引")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int startIndex;

    [Header("循环次数")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int count = 1;

    [Header("当前索引（每轮更新）")]
    [Output(ShowBackingValue.Always)]
    public int index;

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
        BaseNode bodyStart = GetBodyStart();
        if (bodyStart == null)
        {
            NodeLog.Warning($"{GetType().Name}: 循环体未连接");
            yield break;
        }

        int start = GetInputValue<int>(nameof(startIndex), startIndex);
        int total = Mathf.Max(0, GetInputValue<int>(nameof(count), count));

        for (int i = start; i < start + total; i++)
        {
            index = i;
            yield return GraphChainRunner.RunChain(graph as BaseNodeGraph, bodyStart, loopHost, null);
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(startIndex))
            return GetInputValue<int>(nameof(startIndex), startIndex);
        if (port.fieldName == nameof(count))
            return GetInputValue<int>(nameof(count), count);
        if (port.fieldName == nameof(index))
            return index;
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