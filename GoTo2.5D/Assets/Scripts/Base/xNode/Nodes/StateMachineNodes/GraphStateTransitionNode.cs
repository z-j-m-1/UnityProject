using UnityEngine;
using XNode;

/// <summary>
/// 状态机-切换状态节点：让目标 GraphStateMachine 切换到指定状态。
/// 通常放在某个状态子图内部，作为该状态的流转出口。
/// </summary>
[CreateNodeMenu("状态机/切换")]
[NodeTint("#AA66CC")]
public class GraphStateTransitionNode : FlowNode
{
    [Header("状态机名（空 = 图绑定物体上查找）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string machineName;

    [Header("目标状态名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string targetState;

    public override void Execute()
    {
        string machine = GetInputValue<string>(nameof(machineName), machineName);
        string state = GetInputValue<string>(nameof(targetState), targetState);

        GraphStateMachine sm = FindMachine(machine);
        if (sm == null)
        {
            NodeLog.Error($"{GetType().Name}: 找不到状态机 '{machine}'");
            return;
        }
        sm.TransitionTo(state);
        base.Execute();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(machineName))
            return GetInputValue<string>(nameof(machineName), machineName);
        if (port.fieldName == nameof(targetState))
            return GetInputValue<string>(nameof(targetState), targetState);
        return null;
    }

    private GraphStateMachine FindMachine(string machineName)
    {
        BaseNodeGraph nodeGraph = graph as BaseNodeGraph;
        if (string.IsNullOrEmpty(machineName))
        {
            // 图绑定物体（或其父级）上查找状态机
            GameObject attached = nodeGraph != null ? nodeGraph.GetAttachedObject() : null;
            if (attached != null)
            {
                return attached.GetComponentInParent<GraphStateMachine>();
            }
            return null;
        }

        GraphStateMachine[] all = Object.FindObjectsOfType<GraphStateMachine>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == machineName)
            {
                return all[i];
            }
        }
        return null;
    }
}