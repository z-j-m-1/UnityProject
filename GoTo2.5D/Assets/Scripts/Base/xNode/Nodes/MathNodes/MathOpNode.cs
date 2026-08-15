using UnityEngine;
using XNode;

/// <summary>数学运算类型</summary>
public enum MathOperation
{
    Add,
    Subtract,
    Multiply,
    Divide
}

/// <summary>
/// 数学运算节点基类 - 对 a、b 做四则运算输出 result
/// 输入可接线也可在节点上填默认值；不参与执行链（纯数据节点）
/// </summary>
/// <typeparam name="T">运算类型（int / float）</typeparam>
public abstract class MathOpNode<T> : DataNode
{
    [Header("运算")]
    public MathOperation operation = MathOperation.Add;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T a;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T b;

    [Output]
    public T result;

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
            result = Calculate(va, vb);
            return result;
        }
        return null;
    }

    /// <summary>子类实现具体运算（含除零保护）</summary>
    protected abstract T Calculate(T a, T b);
}
