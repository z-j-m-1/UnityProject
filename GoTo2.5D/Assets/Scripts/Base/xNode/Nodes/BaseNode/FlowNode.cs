using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

// 流程节点
[NodeTint("#4488FF")]
public abstract class FlowNode : BaseNode
{
    [Input(backingValue = ShowBackingValue.Never)] 
    public BaseNode input;
    [Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)] 
    public BaseNode next;

    public override BaseNode GetConnectedNode()
    {
        NodePort port = GetOutputPort(nameof(next));
        if (port != null && port.IsConnected)
        {
            NodePort connection = port.GetConnection(0);
            if (connection != null)
                return connection.node as BaseNode;
        }
        return null;
    }

    public override void Execute()
    {
        Debug.Log($"FlowNode: {name} 执行");
    }
}
