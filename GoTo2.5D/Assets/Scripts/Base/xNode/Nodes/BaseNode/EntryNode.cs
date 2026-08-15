using UnityEngine;
using XNode;

/// <summary>
/// 入口节点 - 与 StartNode 类似，但带字符串标识符，同一张图可放多个
/// GraphExecutor 选择"入口执行"时按标识符 / GUID 从该节点开始执行
/// </summary>
[CreateNodeMenu("基本/入口")]
[NodeTint("#AA77DD")]
public class EntryNode : BaseNode, ISerializationCallbackReceiver
{
    [Header("入口标识符（必填，图中唯一）")]
    public string identifier;

    [Header("入口 GUID（自动生成，改名兜底用）")]
    public string guid;

    [Output(backingValue = ShowBackingValue.Never, connectionType = ConnectionType.Override)]
    public BaseNode next;

    public string Identifier => identifier;

    public string Guid => EnsureGuid();

    private string EnsureGuid()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = System.Guid.NewGuid().ToString();
        }
        return guid;
    }

    public void OnBeforeSerialize()
    {
        EnsureGuid();
    }

    public void OnAfterDeserialize()
    {
        EnsureGuid();
    }

    public override BaseNode GetConnectedNode()
    {
        NodePort port = GetOutputPort(nameof(next));
        if (port != null && port.IsConnected)
        {
            NodePort connection = port.GetConnection(0);
            if (connection != null)
            {
                return connection.node as BaseNode;
            }
        }
        return null;
    }

    public override void Execute()
    {
        Debug.Log($"EntryNode: {name} 执行");
    }
}
