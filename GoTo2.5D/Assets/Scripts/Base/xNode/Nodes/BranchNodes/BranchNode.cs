using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using XNode;

[CreateNodeMenu("流程分支/双分支")]
public class BranchNode : FlowNode
{
    [Input]
    public bool condition;
    [Output]
    public BaseNode falseTo;
    public override BaseNode GetConnectedNode()
    {
        NodePort truePort = GetOutputPort(nameof(next));
        NodePort falsePort = GetOutputPort(nameof(falseTo));

        // 按条件选中分支端口
        bool value = GetInputValue<bool>(nameof(condition), condition);
        NodePort selected = value ? truePort : falsePort;

        // 端口缺失 / 该分支未连线：安全结束当前链（不再抛 NullReferenceException）
        if (selected == null || !selected.IsConnected)
        {
            return null;
        }

        NodePort connection = selected.GetConnection(0);
        return connection != null ? connection.node as BaseNode : null;
    }
}
