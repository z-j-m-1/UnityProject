using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public abstract class ValueNode<T> : DataNode
{
    [Output(ShowBackingValue.Always)]
    public T Value;


    public override object GetValue(NodePort port)
    {
        if(port.fieldName == "Value")
        {
            return Value;
        }
        return null;
    }
}
