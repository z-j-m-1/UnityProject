using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 字符串-拼接：把 a / b / c 三段按顺序拼成一个字符串，段与段之间插入 separator（可不接）。
/// skipEmpty（默认开）跳过空段——未接线的段不会留下多余分隔符；
/// 关掉则严格输出 a + sep + b + sep + c（用于故意保留空位）。
/// 需要更多段时串联本节点即可。纯数据节点。
/// </summary>
[CreateNodeMenu("字符串/拼接")]
public class StringConcatNode : DataNode
{
    [Header("段间分隔符（可选）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string separator;

    [Header("文本段")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string a;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string b;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string c;

    [Header("跳过空段")]
    public bool skipEmpty = true;

    [Output]
    public string result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(separator)) return GetInputValue<string>(nameof(separator), separator);
        if (port.fieldName == nameof(a)) return GetInputValue<string>(nameof(a), a);
        if (port.fieldName == nameof(b)) return GetInputValue<string>(nameof(b), b);
        if (port.fieldName == nameof(c)) return GetInputValue<string>(nameof(c), c);

        if (port.fieldName == nameof(result))
        {
            string sep = GetInputValue<string>(nameof(separator), separator) ?? "";
            string va = GetInputValue<string>(nameof(a), a);
            string vb = GetInputValue<string>(nameof(b), b);
            string vc = GetInputValue<string>(nameof(c), c);

            if (!skipEmpty)
            {
                result = (va ?? "") + sep + (vb ?? "") + sep + (vc ?? "");
            }
            else
            {
                List<string> parts = new List<string>(3);
                if (!string.IsNullOrEmpty(va)) parts.Add(va);
                if (!string.IsNullOrEmpty(vb)) parts.Add(vb);
                if (!string.IsNullOrEmpty(vc)) parts.Add(vc);
                result = string.Join(sep, parts);
            }
            return result;
        }
        return null;
    }
}
