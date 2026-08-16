using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 流程-跳转到入口：执行目标入口（entryId）开头的链，执行完后当前链结束（不再沿 next 继续）。
/// 适合"出错重试 / 直接切到某段逻辑"；目标入口不存在时报错并直接结束当前链。
/// </summary>
[CreateNodeMenu("流程/跳转到入口")]
[NodeTint("#44CC88")]
public class JumpToEntryNode : FlowNode
{
    [Header("目标入口标识（与 EntryNode 的标识符一致）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string entryId;

    private MonoBehaviour loopHost;
    private EntryNode target;

    public override void Execute()
    {
        loopHost = NodeExecuteContext.Current;
        string id = GetInputValue<string>(nameof(entryId), entryId);
        target = graph is BaseNodeGraph g ? g.GetEntryNode(id) : null;
        if (target == null)
        {
            NodeLog.Error($"{GetType().Name}: 入口 '{id}' 未找到，当前链将结束");
        }
        base.Execute();
    }

    public override IEnumerator GetFlow()
    {
        if (target != null)
        {
            yield return GraphChainRunner.RunChain(graph as BaseNodeGraph, target, loopHost, null);
        }
    }

    public override BaseNode GetConnectedNode()
    {
        return null; // 跳转后当前链结束
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(entryId))
            return GetInputValue<string>(nameof(entryId), entryId);
        return null;
    }
}