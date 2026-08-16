using UnityEngine;
using XNode;

/// <summary>取值-转换：二维向量合成三维向量（z = 0，输入可接线或节点上填值）</summary>
[CreateNodeMenu("取值/转换/三维向量(二维向量)")]
public class Vector2ToVector3Node : DataNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector2 vector2;

    [Output]
    public Vector3 vector3;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(vector2))
            return GetInputValue<Vector2>(nameof(vector2), vector2);
        if (port.fieldName == nameof(vector3))
        {
            Vector2 v = GetInputValue<Vector2>(nameof(vector2), vector2);
            vector3 = new Vector3(v.x, v.y, 0f);
            return vector3;
        }
        return null;
    }
}