using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("逻辑门/或门")]
public class OrLogicNode : LogicNode
{
    public override object GetValue(NodePort port)
    {
        return GetInputValue<bool>("conditionOne", conditionOne) || GetInputValue<bool>("conditionTwo", conditionTwo);
    }
}
