using UnityEngine;
using XNode;

/// <summary>取值-转换：两个浮点合成一个二维向量（输入可接线或节点上填值）</summary>
[CreateNodeMenu("取值/转换/二维向量(两个浮点)")]
public class Float2ToVector2Node : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float x;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float y;

    [Output]
    public Vector2 vector;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(x))
            return GetInputValue<float>(nameof(x), x);
        if (port.fieldName == nameof(y))
            return GetInputValue<float>(nameof(y), y);
        if (port.fieldName == nameof(vector))
        {
            vector = new Vector2(
                GetInputValue<float>(nameof(x), x),
                GetInputValue<float>(nameof(y), y));
            return vector;
        }
        return null;
    }
}