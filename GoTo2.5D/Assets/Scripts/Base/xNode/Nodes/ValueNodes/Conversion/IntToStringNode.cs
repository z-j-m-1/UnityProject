using XNode;

/// <summary>取值-转换：整数 → 字符串</summary>
[CreateNodeMenu("取值/转换/字符串(整数)")]
public class IntToStringNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int input;

    [Output]
    public string result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(input)) return GetInputValue<int>(nameof(input), input);
        if (port.fieldName == nameof(result))
        {
            result = GetInputValue<int>(nameof(input), input).ToString();
            return result;
        }
        return null;
    }
}
