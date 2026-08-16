using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 共享链执行器：沿 GetConnectedNode 走一条链（每节点 Execute + yield GetFlow + 步进），
/// 供 GraphExecutor（主链）与 SubGraphNode（子图链）复用，保证两种链行为一致。
/// </summary>
public static class GraphChainRunner
{
    /// <summary>
    /// 走一条链。
    /// </summary>
    /// <param name="graph">所在图（用于日志）</param>
    /// <param name="start">起点节点</param>
    /// <param name="host">宿主 MonoBehaviour（执行器 / 状态机等）：子图链传父宿主（null 则节点上下文为空），保证目标解析一致</param>
    /// <param name="onCurrentNode">每步执行前回调（运行高亮用）</param>
    /// <param name="maxLoop">链长上限（防死循环）</param>
    public static IEnumerator RunChain(BaseNodeGraph graph, BaseNode start, MonoBehaviour host, Action<BaseNode> onCurrentNode, int maxLoop = 100)
    {
        BaseNode node = start;
        int counter = 0;

        while (node != null && counter < maxLoop)
        {
            if (onCurrentNode != null)
            {
                onCurrentNode(node);
            }

            // 执行上下文：让节点能解析到"当前执行器"的目标物体
            NodeExecuteContext.Current = host;
            try
            {
                node.Execute();
            }
            finally
            {
                NodeExecuteContext.Current = null;
            }

            // 节点可返回协程流程（等待/等待条件/子图等），yield 暂停链直到完成
            IEnumerator flow = node.GetFlow();
            if (flow != null)
            {
                yield return flow;
            }

            node = node.GetConnectedNode();
            counter++;
        }

        if (counter >= maxLoop)
        {
            Debug.LogWarning($"图 '{graph?.name}': 执行达到最大循环次数");
        }
    }
}
