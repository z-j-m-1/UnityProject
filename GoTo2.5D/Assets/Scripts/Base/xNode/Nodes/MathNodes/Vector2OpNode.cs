using UnityEngine;
using XNode;

/// <summary>数学运算-二维向量：加 / 减（输入可接线或节点上填值）</summary>
[CreateNodeMenu("数学运算/二维向量运算")]
public class Vector2OpNode : DataNode
{
    [Header("运算")]
    public MathOperation operation = MathOperation.Add;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector2 a;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector2 b;

    [Output]
    public Vector2 result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(a))
            return GetInputValue<Vector2>(nameof(a), a);
        if (port.fieldName == nameof(b))
            return GetInputValue<Vector2>(nameof(b), b);
        if (port.fieldName == nameof(result))
        {
            Vector2 va = GetInputValue<Vector2>(nameof(a), a);
            Vector2 vb = GetInputValue<Vector2>(nameof(b), b);
            result = operation == MathOperation.Subtract ? va - vb : va + vb;
            return result;
        }
        return null;
    }
}