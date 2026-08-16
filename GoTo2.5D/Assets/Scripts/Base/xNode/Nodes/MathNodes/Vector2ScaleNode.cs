using UnityEngine;
using XNode;

/// <summary>数学运算-二维向量缩放：向量 × 标量（输入可接线或节点上填值）</summary>
[CreateNodeMenu("数学运算/二维向量缩放")]
public class Vector2ScaleNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector2 vector;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float scalar = 1f;

    [Output]
    public Vector2 result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(vector))
            return GetInputValue<Vector2>(nameof(vector), vector);
        if (port.fieldName == nameof(scalar))
            return GetInputValue<float>(nameof(scalar), scalar);
        if (port.fieldName == nameof(result))
        {
            result = GetInputValue<Vector2>(nameof(vector), vector) * GetInputValue<float>(nameof(scalar), scalar);
            return result;
        }
        return null;
    }
}