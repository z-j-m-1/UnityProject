using XNode;

/// <summary>取值-转换：字符串 → 浮点（失败返回 0）</summary>
[CreateNodeMenu("取值/转换/字符串转浮点")]
public class StringToFloatNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string input;

    [Output]
    public float result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(input)) return GetInputValue<string>(nameof(input), input);
        if (port.fieldName == nameof(result))
        {
            float.TryParse(GetInputValue<string>(nameof(input), input), out result);
            return result;
        }
        return null;
    }
}
