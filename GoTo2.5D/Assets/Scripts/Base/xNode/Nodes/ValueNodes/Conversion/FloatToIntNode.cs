using UnityEngine;
using XNode;

/// <summary>取值-转换：浮点 → 整数（四舍五入）</summary>
[CreateNodeMenu("取值/转换/整数(浮点)")]
public class FloatToIntNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float input;

    [Output]
    public int result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(input)) return GetInputValue<float>(nameof(input), input);
        if (port.fieldName == nameof(result))
        {
            result = Mathf.RoundToInt(GetInputValue<float>(nameof(input), input));
            return result;
        }
        return null;
    }
}
