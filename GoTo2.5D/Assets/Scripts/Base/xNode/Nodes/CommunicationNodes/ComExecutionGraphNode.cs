using UnityEngine;
using XNode;
using ZGameFramework.Core;

/// <summary>
/// 通讯-执行节点图节点
/// </summary>
[CreateNodeMenu("通讯/执行节点图")]
public class ComExecutionGraphNode : FlowNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string graphName = "";

    public override void Execute()
    {
        string targetGraphName = GetInputValue<string>("graphName", this.graphName);

        if (string.IsNullOrEmpty(targetGraphName))
        {
            Debug.LogError("ComExecutionGraphNode: 目标节点图名称不能为空");
            return;
        }

        ComExecutionGraphEvent.Trigger(evt =>
        {
            evt.graphName = targetGraphName;
        });

        NodeLog.Info($"ComExecutionGraphNode: 触发通讯执行节点图事件 - 图:'{targetGraphName}'");

        base.Execute();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(graphName))
            return GetInputValue<string>("graphName", graphName);
        return null;
    }
}

/// <summary>
/// 通讯-执行节点图事件
/// </summary>
public class ComExecutionGraphEvent : ParameterizedEvent<ComExecutionGraphEvent>
{
    public string graphName;

    public override void OnRecycled()
    {
        graphName = null;
    }
}