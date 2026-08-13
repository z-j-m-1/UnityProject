using UnityEngine;
using XNode;

[CreateNodeMenu("流程分支/多分支")]
public class MultiBranchNode : FlowNode
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public bool[] conditions;

    [Output(dynamicPortList = true)]
    public BaseNode[] outputs;

    public override BaseNode GetConnectedNode()
    {
        // 1. 获取输入的条件数组
        bool[] inputConditions = GetInputValue<bool[]>("conditions", conditions);

        // 2. 如果没有条件，走 next
        if (inputConditions == null || inputConditions.Length == 0)
        {
            return GetNextNode();
        }

        // 3. 查找第一个 true 的索引
        int selectedIndex = -1;
        for (int i = 0; i < inputConditions.Length; i++)
        {
            if (inputConditions[i])
            {
                selectedIndex = i;
                break;
            }
        }

        // 4. 所有条件都是 false，走 next
        if (selectedIndex == -1)
        {
            return GetNextNode();
        }

        // 5. 索引超出范围，走 next
        if (selectedIndex >= outputs.Length)
        {
            return GetNextNode();
        }

        // ✅ 方法1：直接通过数组访问（最简单可靠）
        if (outputs[selectedIndex] != null)
        {
            return outputs[selectedIndex];
        }

        // ✅ 方法2：通过端口名称访问（注意空格！）
        string portName = $"outputs {selectedIndex}";  // 关键：空格！
        NodePort port = GetPort(portName);

        if (port != null && port.Connection != null)
        {
            BaseNode targetNode = port.Connection.node as BaseNode;
            if (targetNode != null)
            {
                return targetNode;
            }
        }

        // 分支未连接，走 next
        return GetNextNode();
    }

    private BaseNode GetNextNode()
    {
        NodePort nextPort = GetPort("next");
        if (nextPort != null && nextPort.Connection != null)
        {
            return nextPort.Connection.node as BaseNode;
        }
        return null;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(conditions))
        {
            return GetInputValue<bool[]>("conditions", conditions);
        }
        return null;
    }
}