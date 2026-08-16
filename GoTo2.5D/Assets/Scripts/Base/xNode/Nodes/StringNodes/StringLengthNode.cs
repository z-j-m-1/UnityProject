using UnityEngine;
using XNode;

/// <summary>字符串长度节点（输出 int），纯数据节点</summary>
[CreateNodeMenu("字符串/长度")]
public class StringLengthNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string input;

    [Output]
    public int length;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(input)) return GetInputValue<string>(nameof(input), input);
        if (port.fieldName == nameof(length))
        {
            string v = GetInputValue<string>(nameof(input), input);
            length = v != null ? v.Length : 0;
            return length;
        }
        return null;
    }
}
