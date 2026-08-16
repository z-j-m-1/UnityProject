using UnityEngine;
using XNode;

/// <summary>数学运算-随机整数（含上限）</summary>
[CreateNodeMenu("数学运算/随机整数")]
public class RandomIntNode : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int min;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int max = 100;

    [Output]
    public int result;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(min)) return GetInputValue<int>(nameof(min), min);
        if (port.fieldName == nameof(max)) return GetInputValue<int>(nameof(max), max);
        if (port.fieldName == nameof(result))
        {
            int a = GetInputValue<int>(nameof(min), min);
            int b = GetInputValue<int>(nameof(max), max);
            result = Random.Range(a, b + 1);
            return result;
        }
        return null;
    }
}
