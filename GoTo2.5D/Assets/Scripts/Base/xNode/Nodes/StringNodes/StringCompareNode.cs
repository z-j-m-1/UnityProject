using UnityEngine;
using XNode;

/// <summary>字符串比较类型</summary>
public enum StringCompareOperation
{
    Equals,
    Contains,
    StartsWith,
    EndsWith
}

/// <summary>字符串比较节点（等于/包含/以…开头/以…结尾，输出 bool），纯数据节点</summary>
[CreateNodeMenu("字符串/比较")]
public class StringCompareNode : DataNode
{
    [Header("比较")]
    public StringCompareOperation operation = StringCompareOperation.Equals;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string a;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string b;

    [Output]
    public bool result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(a)) return GetInputValue<string>(nameof(a), a);
        if (port.fieldName == nameof(b)) return GetInputValue<string>(nameof(b), b);
        if (port.fieldName == nameof(result))
        {
            string va = GetInputValue<string>(nameof(a), a) ?? "";
            string vb = GetInputValue<string>(nameof(b), b) ?? "";
            switch (operation)
            {
                case StringCompareOperation.Equals: result = va == vb; break;
                case StringCompareOperation.Contains: result = va.Contains(vb); break;
                case StringCompareOperation.StartsWith: result = va.StartsWith(vb); break;
                case StringCompareOperation.EndsWith: result = va.EndsWith(vb); break;
            }
            return result;
        }
        return null;
    }
}
