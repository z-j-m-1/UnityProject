using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通讯-获取整数变量节点
/// </summary>
[CreateNodeMenu("通讯/获取/整数")]
public class ComGetIntVariableNode : ComGetVariableNode<int>
{
    protected override void RequestVariable(string graphName, string varName, System.Action<int> callback)
    {
        ComGetVariableEvent<int>.Trigger(evt =>
        {
            evt.targetGraphName = graphName;
            evt.variableName = varName;
            evt.defaultValue = defaultValue;
            evt.callback = callback;
        });
    }
}
