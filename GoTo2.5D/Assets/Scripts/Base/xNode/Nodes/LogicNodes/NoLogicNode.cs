using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using XNode;

[CreateNodeMenu("逻辑门/非门")]
public class NoLogicNode : DataNode
{
    [Input]
    public bool condition;

    [Output]
    public bool result;
    public override object GetValue(NodePort port)
    {
        return !condition;
    }
}
