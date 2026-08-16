using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 列表-是否包含（数据节点）：list 包含 item 输出 true（null 列表视为不包含）。
/// </summary>
public abstract class ListContainsNode<T> : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public List<T> list;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T item;

    [Output]
    public bool contains;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(list))
            return GetInputValue<List<T>>(nameof(list), list);
        if (port.fieldName == nameof(item))
            return GetInputValue<T>(nameof(item), item);
        if (port.fieldName == nameof(contains))
        {
            List<T> target = GetInputValue<List<T>>(nameof(list), list);
            contains = target != null && target.Contains(GetInputValue<T>(nameof(item), item));
            return contains;
        }
        return null;
    }
}