using UnityEngine;
using XNode;

/// <summary>取值-转换：三维向量取 x/y 成二维向量（z 丢弃，输入可接线或节点上填值）</summary>
[CreateNodeMenu("取值/转换/二维向量(三维向量)")]
public class Vector3ToVector2Node : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 vector3;

    [Output]
    public Vector2 vector2;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(vector3))
            return GetInputValue<Vector3>(nameof(vector3), vector3);
        if (port.fieldName == nameof(vector2))
        {
            Vector3 v = GetInputValue<Vector3>(nameof(vector3), vector3);
            vector2 = new Vector2(v.x, v.y);
            return vector2;
        }
        return null;
    }
}