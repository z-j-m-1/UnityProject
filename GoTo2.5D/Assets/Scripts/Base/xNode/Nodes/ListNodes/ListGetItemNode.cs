using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 列表-取元素（数据节点）：按 index 取 list 中的元素，越界返回默认值并警告。
/// </summary>
public abstract class ListGetItemNode<T> : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public List<T> list;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int index;

    [Output]
    public T item;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(list))
            return GetInputValue<List<T>>(nameof(list), list);
        if (port.fieldName == nameof(index))
            return GetInputValue<int>(nameof(index), index);
        if (port.fieldName == nameof(item))
        {
            List<T> target = GetInputValue<List<T>>(nameof(list), list);
            int idx = GetInputValue<int>(nameof(index), index);
            if (target == null || idx < 0 || idx >= target.Count)
            {
                NodeLog.Warning($"{GetType().Name}: 索引 {idx} 越界（列表长度 {(target != null ? target.Count : 0)}），返回默认值");
                item = default;
                return item;
            }
            item = target[idx];
            return item;
        }
        return null;
    }
}