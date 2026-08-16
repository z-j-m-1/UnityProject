using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 列表-数量（数据节点）：输出 list 的长度（null 视为 0）。
/// </summary>
public abstract class ListCountNode<T> : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public List<T> list;

    [Output]
    public int count;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(list))
            return GetInputValue<List<T>>(nameof(list), list);
        if (port.fieldName == nameof(count))
        {
            List<T> target = GetInputValue<List<T>>(nameof(list), list);
            count = target != null ? target.Count : 0;
            return count;
        }
        return null;
    }
}