using UnityEngine;
using XNode;

/// <summary>字符串运算类型</summary>
public enum StringOperation
{
    Concat,
    ToUpper,
    ToLower,
    Trim
}

/// <summary>字符串运算节点（拼接/转大写/转小写/去空格），纯数据节点</summary>
[CreateNodeMenu("字符串/运算")]
public class StringOpNode : DataNode
{
    [Header("运算")]
    public StringOperation operation = StringOperation.Concat;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string a;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string b;

    [Output]
    public string result;

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
                case StringOperation.Concat: result = va + vb; break;
                case StringOperation.ToUpper: result = va.ToUpper(); break;
                case StringOperation.ToLower: result = va.ToLower(); break;
                case StringOperation.Trim: result = va.Trim(); break;
            }
            return result;
        }
        return null;
    }
}
