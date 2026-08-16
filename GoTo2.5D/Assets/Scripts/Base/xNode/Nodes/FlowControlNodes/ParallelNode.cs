using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 流程-并行：同时启动最多 4 条分支链（branchA–D，各自以 EndNode 收尾），全部完成后沿 next 继续。
/// 并行链在宿主（执行器/状态机）协程上运行，真正交错执行；未连接的分支跳过。
/// </summary>
[CreateNodeMenu("流程/并行")]
[NodeTint("#44CC88")]
public class ParallelNode : FlowNode
{
    [Header("并行分支（最多 4 条，各自以 EndNode 收尾）")]
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public BaseNode branchA;
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public BaseNode branchB;
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public BaseNode branchC;
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public BaseNode branchD;

    private MonoBehaviour loopHost;

    public override void Execute()
    {
        loopHost = NodeExecuteContext.Current;
        base.Execute();
    }

    public override IEnumerator GetFlow()
    {
        List<BaseNode> branches = new List<BaseNode>();
        for (int i = 0; i < 4; i++)
        {
            BaseNode b = GetBranch(i);
            if (b != null) branches.Add(b);
        }
        if (branches.Count == 0)
        {
            NodeLog.Warning($"{GetType().Name}: 未连接任何并行分支");
            yield break;
        }

        if (loopHost == null)
        {
            // 无宿主（编辑模式等）：顺序执行分支
            foreach (BaseNode b in branches)
            {
                yield return GraphChainRunner.RunChain(graph as BaseNodeGraph, b, null, null);
            }
            yield break;
        }

        int remaining = branches.Count;
        foreach (BaseNode b in branches)
        {
            BaseNode start = b;
            loopHost.StartCoroutine(RunBranch(start, () => remaining--));
        }
        yield return new WaitUntil(() => remaining <= 0);
    }

    private IEnumerator RunBranch(BaseNode start, System.Action done)
    {
        yield return GraphChainRunner.RunChain(graph as BaseNodeGraph, start, loopHost, null);
        done();
    }

    private BaseNode GetBranch(int i)
    {
        string field = i == 0 ? nameof(branchA) : i == 1 ? nameof(branchB) : i == 2 ? nameof(branchC) : nameof(branchD);
        NodePort port = GetOutputPort(field);
        if (port != null && port.IsConnected)
        {
            NodePort connection = port.GetConnection(0);
            if (connection != null) return connection.node as BaseNode;
        }
        return null;
    }
}