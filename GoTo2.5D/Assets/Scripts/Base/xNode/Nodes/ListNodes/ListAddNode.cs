using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 列表-添加元素（流程节点）：把 item 追加到 list 末尾。
/// list 通常接「获取列表变量」或子图列表参数输入；未接线时用节点自身字段（默认空）。
/// 注意：list 取到的是变量引用，直接修改即写回变量（无需再 Set）。
/// </summary>
public abstract class ListAddNode<T> : FlowNode
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
            NodeLog.Warning($"{GetType().Name}: 列表为空（list 未接线且节点上无默认列表），添加被忽略");
            return;
        }
        target.Add(GetInputValue<T>(nameof(item), item));
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