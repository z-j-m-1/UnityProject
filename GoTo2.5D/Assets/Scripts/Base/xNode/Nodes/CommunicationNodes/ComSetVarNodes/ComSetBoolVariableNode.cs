using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateNodeMenu("通讯/设置/布尔")]
public class ComSetBoolVariableNode : ComSetVariableNode<bool>
{
    protected override void TriggerEvent(string graphName, string varName, bool varValue)
    {
        ComSetBoolVariableEvent.Trigger(evt =>
        {
            evt.targetGraphName = graphName;
            evt.variableName = varName;
            evt.variableValue = varValue;
        });
    }
}
