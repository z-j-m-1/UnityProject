using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 流程-条件循环：每轮先检查 condition，为 true 则执行循环体（body 子链，以 EndNode 收尾），
/// 之后回到条件判断；condition 为 false 时沿 next 继续。
/// maxIterations 防死循环（条件可由循环体改的变量驱动，也可能永远为 true）。
/// </summary>
[CreateNodeMenu("流程/条件循环")]
[NodeTint("#44CC88")]
public class WhileLoopNode : FlowNode
{
    [Header("循环条件（可接比较/逻辑/变量节点）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public bool condition;

    [Header("循环体（每轮执行，用 EndNode 收尾）")]
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public BaseNode body;

    [Header("最大迭代次数（防死循环）")]
    public int maxIterations = 1000;

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

        int guard = Mathf.Max(1, maxIterations);
        int iter = 0;
        while (GetInputValue<bool>(nameof(condition), condition))
        {
            iter++;
            if (iter > guard)
            {
                NodeLog.Warning($"{GetType().Name}: 达到最大迭代次数 {guard}，疑似死循环，已退出");
                yield break;
            }
            yield return GraphChainRunner.RunChain(graph as BaseNodeGraph, bodyStart, loopHost, null);
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(condition))
            return GetInputValue<bool>(nameof(condition), condition);
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