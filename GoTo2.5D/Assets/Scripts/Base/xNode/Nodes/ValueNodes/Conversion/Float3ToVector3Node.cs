using UnityEngine;
using XNode;

/// <summary>取值-转换：三个浮点合成一个三维向量（输入可接线或节点上填值）</summary>
[CreateNodeMenu("取值/转换/三维向量(三个浮点)")]
public class Float3ToVector3Node : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float x;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float y;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float z;

    [Output]
    public Vector3 vector;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(x))
            return GetInputValue<float>(nameof(x), x);
        if (port.fieldName == nameof(y))
            return GetInputValue<float>(nameof(y), y);
        if (port.fieldName == nameof(z))
            return GetInputValue<float>(nameof(z), z);
        if (port.fieldName == nameof(vector))
        {
            vector = new Vector3(
                GetInputValue<float>(nameof(x), x),
                GetInputValue<float>(nameof(y), y),
                GetInputValue<float>(nameof(z), z));
            return vector;
        }
        return null;
    }
}
