using System;
using UnityEngine;
using XNode;

/// <summary>比较运算类型</summary>
public enum CompareOperation
{
    Equal,
    NotEqual,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual
}

/// <summary>
/// 比较运算节点基类 - 比较 a、b 输出 bool
/// 输入可接线也可在节点上填默认值；不参与执行链（纯数据节点）
/// </summary>
/// <typeparam name="T">比较类型（int / float）</typeparam>
public abstract class CompareNode<T> : DataNode where T : IComparable<T>
{
    [Header("比较")]
    public CompareOperation operation = CompareOperation.Greater;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T a;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T b;

    [Output]
    public bool result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(a))
            return GetInputValue<T>(nameof(a), a);
        if (port.fieldName == nameof(b))
            return GetInputValue<T>(nameof(b), b);
        if (port.fieldName == nameof(result))
        {
            T va = GetInputValue<T>(nameof(a), a);
            T vb = GetInputValue<T>(nameof(b), b);
            int cmp = va.CompareTo(vb);
            switch (operation)
            {
                case CompareOperation.Equal: result = cmp == 0; break;
                case CompareOperation.NotEqual: result = cmp != 0; break;
                case CompareOperation.Greater: result = cmp > 0; break;
                case CompareOperation.GreaterOrEqual: result = cmp >= 0; break;
                case CompareOperation.Less: result = cmp < 0; break;
                case CompareOperation.LessOrEqual: result = cmp <= 0; break;
            }
            return result;
        }
        return null;
    }
}
