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
        NodePort tureTo = GetOutputPort(nameof(next));
        NodePort falseTo = GetOutputPort(nameof(falseTo));
        return ((GetInputValue<bool>("condition", condition) ? tureTo : falseTo).GetConnection(0).node) as BaseNode;
    }

        
    
}
