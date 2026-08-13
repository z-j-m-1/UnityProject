using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using XNode;

public abstract class LogicNode : DataNode
{
    [Input]
    public bool conditionOne;
    [Input]
    public bool conditionTwo;

    [Output]
    public bool result;
    public abstract override object GetValue(NodePort port);
}
