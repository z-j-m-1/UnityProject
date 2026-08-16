using XNode;

/// <summary>取值-转换：整数 → 浮点</summary>
[CreateNodeMenu("取值/转换/浮点(整数)")]
public class IntToFloatNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int input;

    [Output]
    public float result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(input)) return GetInputValue<int>(nameof(input), input);
        if (port.fieldName == nameof(result))
        {
            result = GetInputValue<int>(nameof(input), input);
            return result;
        }
        return null;
    }
}
