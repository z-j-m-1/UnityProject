using UnityEngine;
using XNode;

/// <summary>数学运算-随机浮点</summary>
[CreateNodeMenu("数学运算/随机浮点")]
public class RandomFloatNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float min;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float max = 1f;

    [Output]
    public float result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(min)) return GetInputValue<float>(nameof(min), min);
        if (port.fieldName == nameof(max)) return GetInputValue<float>(nameof(max), max);
        if (port.fieldName == nameof(result))
        {
            float a = GetInputValue<float>(nameof(min), min);
            float b = GetInputValue<float>(nameof(max), max);
            result = Random.Range(a, b);
            return result;
        }
        return null;
    }
}
