using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 流程-计时器：每隔 interval 秒执行一次 tick 子链（以 EndNode 收尾），共执行 times 次（0 = 无限循环）；
/// 次数到后沿 next 继续。无限循环时靠停止链/切换状态来终止。
/// </summary>
[CreateNodeMenu("流程/计时器")]
[NodeTint("#44CC88")]
public class TimerNode : FlowNode
{
    [Header("间隔秒数")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float interval = 1f;

    [Header("执行次数（0 = 无限循环）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int times = 1;

    [Header("每次间隔执行（用 EndNode 收尾）")]
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public BaseNode tick;

    private MonoBehaviour loopHost;

    public override void Execute()
    {
        loopHost = NodeExecuteContext.Current;
        base.Execute();
    }

    public override IEnumerator GetFlow()
    {
        BaseNode tickStart = GetTickStart();
        if (tickStart == null)
        {
            NodeLog.Warning($"{GetType().Name}: tick 子链未连接");
            yield break;
        }

        float iv = Mathf.Max(0f, GetInputValue<float>(nameof(interval), interval));
        int total = Mathf.Max(0, GetInputValue<int>(nameof(times), times));

        if (total <= 0)
        {
            // 无限循环：每次等待后跑一次 tick
            while (true)
            {
                yield return new WaitForSeconds(iv);
                yield return GraphChainRunner.RunChain(graph as BaseNodeGraph, tickStart, loopHost, null);
            }
        }

        for (int i = 0; i < total; i++)
        {
            yield return new WaitForSeconds(iv);
            yield return GraphChainRunner.RunChain(graph as BaseNodeGraph, tickStart, loopHost, null);
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(interval))
            return GetInputValue<float>(nameof(interval), interval);
        if (port.fieldName == nameof(times))
            return GetInputValue<int>(nameof(times), times);
        return null;
    }

    private BaseNode GetTickStart()
    {
        NodePort port = GetOutputPort(nameof(tick));
        if (port != null && port.IsConnected)
        {
            NodePort connection = port.GetConnection(0);
            if (connection != null) return connection.node as BaseNode;
        }
        return null;
    }
}