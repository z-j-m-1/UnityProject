using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateNodeMenu("通讯/获取/布尔")]
public class ComGetBoolVariableNode : ComGetVariableNode<bool>
{
    protected override void RequestVariable(string graphName, string varName, System.Action<bool> callback)
    {
        ComGetBoolVariableEvent.Trigger(evt =>
        {
            evt.targetGraphName = graphName;
            evt.variableName = varName;
            evt.defaultValue = defaultValue;
            evt.callback = callback;
        });
    }
}
