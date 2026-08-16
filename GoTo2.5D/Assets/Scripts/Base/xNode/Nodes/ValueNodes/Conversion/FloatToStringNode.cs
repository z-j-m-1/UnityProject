using XNode;

/// <summary>取值-转换：浮点 → 字符串</summary>
[CreateNodeMenu("取值/转换/字符串(浮点)")]
public class FloatToStringNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float input;

    [Output]
    public string result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(input)) return GetInputValue<float>(nameof(input), input);
        if (port.fieldName == nameof(result))
        {
            result = GetInputValue<float>(nameof(input), input).ToString();
            return result;
        }
        return null;
    }
}
