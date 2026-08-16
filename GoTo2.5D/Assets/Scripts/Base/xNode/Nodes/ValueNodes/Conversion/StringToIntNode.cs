using XNode;

/// <summary>取值-转换：字符串 → 整数（失败返回 0）</summary>
[CreateNodeMenu("取值/转换/整数(字符串)")]
public class StringToIntNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string input;

    [Output]
    public int result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(input)) return GetInputValue<string>(nameof(input), input);
        if (port.fieldName == nameof(result))
        {
            int.TryParse(GetInputValue<string>(nameof(input), input), out result);
            return result;
        }
        return null;
    }
}
