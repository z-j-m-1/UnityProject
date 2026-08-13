using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateNodeMenu("通讯/设置/字符串")]
public class ComSetStringVariableNode : ComSetVariableNode<string>
{
    protected override void TriggerEvent(string graphName, string varName, string varValue)
    {
        ComSetStringVariableEvent.Trigger(evt =>
        {
            evt.targetGraphName = graphName;
            evt.variableName = varName;
            evt.variableValue = varValue;
        });
    }
}
