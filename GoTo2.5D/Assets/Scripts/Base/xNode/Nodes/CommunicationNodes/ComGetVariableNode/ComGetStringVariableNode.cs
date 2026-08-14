using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateNodeMenu("通讯/获取/字符串")]
public class ComGetStringVariableNode : ComGetVariableNode<string>
{
    protected override void RequestVariable(string graphName, string varName, System.Action<string> callback)
    {
        ComGetVariableEvent<string>.Trigger(evt =>
        {
            evt.targetGraphName = graphName;
            evt.variableName = varName;
            evt.defaultValue = defaultValue;
            evt.callback = callback;
        });
    }
}
