using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using XNode;

public abstract class GetVarNode<T> : DataNode
{
    [Input(ShowBackingValue.Unconnected)]
    public string varName;

    [Output(ShowBackingValue.Never)]
    public T Value;

    public override object GetValue(NodePort port)
    {
        if(port.fieldName == "Value")
        {
            Value = (graph as BaseNodeGraph).Get<T>(GetInputValue<string>("varName",varName));
            return Value;
        }
        return null;
    }
   
}
