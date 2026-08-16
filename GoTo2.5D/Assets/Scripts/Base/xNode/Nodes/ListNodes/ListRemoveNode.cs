using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 列表-移除元素（流程节点）：从 list 中移除第一个匹配 item 的元素。
/// list 取到的是变量引用，直接修改即写回变量。
/// </summary>
public abstract class ListRemoveNode<T> : FlowNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public List<T> list;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T item;

    public override void Execute()
    {
        List<T> target = GetInputValue<List<T>>(nameof(list), list);
        if (target == null)
        {
            NodeLog.Warning($"{GetType().Name}: 列表为空（list 未接线且节点上无默认列表），移除被忽略");
            return;
        }
        target.Remove(GetInputValue<T>(nameof(item), item));
        base.Execute();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(list))
            return GetInputValue<List<T>>(nameof(list), list);
        if (port.fieldName == nameof(item))
            return GetInputValue<T>(nameof(item), item);
        return null;
    }
}