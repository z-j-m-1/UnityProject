using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateNodeMenu("通讯/设置/整形")]
public class ComSetIntVariableNode : ComSetVariableNode<int>
{
    protected override void TriggerEvent(string graphName, string varName, int varValue)
    {
        ComSetVariableEvent<int>.Trigger(evt =>
        {
            evt.targetGraphName = graphName;
            evt.variableName = varName;
            evt.variableValue = varValue;
        });
    }
}

