using XNode;

/// <summary>字符串-替换（把 oldValue 全部替换为 newValue）</summary>
[CreateNodeMenu("字符串/替换")]
public class StringReplaceNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string input;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string oldValue;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string newValue;

    [Output]
    public string result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(input)) return GetInputValue<string>(nameof(input), input);
        if (port.fieldName == nameof(oldValue)) return GetInputValue<string>(nameof(oldValue), oldValue);
        if (port.fieldName == nameof(newValue)) return GetInputValue<string>(nameof(newValue), newValue);
        if (port.fieldName == nameof(result))
        {
            string v = GetInputValue<string>(nameof(input), input) ?? "";
            result = v.Replace(GetInputValue<string>(nameof(oldValue), oldValue) ?? "",
                               GetInputValue<string>(nameof(newValue), newValue) ?? "");
            return result;
        }
        return null;
    }
}
