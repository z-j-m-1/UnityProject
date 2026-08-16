using UnityEngine;
using XNode;

[CreateNodeMenu("基本/结束")]
// 结束节点 - 只显示输入端口，隐藏字段值
public class EndNode : BaseNode
{
    [Input(backingValue = ShowBackingValue.Never)] 
    public BaseNode Node;

    public override BaseNode GetConnectedNode()
    {
        return null;
    }

    public override void Execute()
    {
        NodeLog.Verbose($"EndNode: {name} 执行");
    }
}