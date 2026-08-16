using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
[CreateNodeMenu("基本/开始")]
// 开始节点 - 使用DisallowMultipleNodes特性禁止重复添加
[DisallowMultipleNodes]
[NodeTint("#44AA44")] // 绿色调便于识别
public class StartNode : BaseNode
{
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
        NodeLog.Verbose($"StartNode: {name} 执行");
    }
}
