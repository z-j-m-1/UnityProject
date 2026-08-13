using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using XNode;

public abstract class SetVarNode<T> : FlowNode
{
    [Input]
    public string varName;
    [Input]
    public T value;

    public override void Execute()
    {
        (graph as BaseNodeGraph).Set<T>(GetInputValue<string>("varName",varName), GetInputValue<T>("value",value));
    }

}
