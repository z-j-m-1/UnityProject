using UnityEngine;
using XNode;

/// <summary>字符串-截取（start 起 length 个字符；越界安全）</summary>
[CreateNodeMenu("字符串/截取")]
public class StringSubstringNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string input;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int start;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int length = 1;

    [Output]
    public string result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(input)) return GetInputValue<string>(nameof(input), input);
        if (port.fieldName == nameof(start)) return GetInputValue<int>(nameof(start), start);
        if (port.fieldName == nameof(length)) return GetInputValue<int>(nameof(length), length);
        if (port.fieldName == nameof(result))
        {
            string v = GetInputValue<string>(nameof(input), input) ?? "";
            int s = Mathf.Max(0, GetInputValue<int>(nameof(start), start));
            int l = GetInputValue<int>(nameof(length), length);
            result = l <= 0 || s >= v.Length ? "" : (s + l >= v.Length ? v.Substring(s) : v.Substring(s, l));
            return result;
        }
        return null;
    }
}
