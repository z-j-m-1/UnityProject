using UnityEngine;
using XNode;

/// <summary>材质-读取浮点属性：输出 sharedMaterial 上某个浮点属性（如 _Dissolve / _OutlineWidth）。</summary>
[CreateNodeMenu("材质/读取浮点")]
public class GetMaterialFloatNode : GetMaterialPropertyNodeBase
{
    [Header("浮点值")]
    [Output]
    public float floatValue;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(floatValue))
        {
            string name;
            Material material = ResolveMaterial(out name);
            floatValue = material != null ? material.GetFloat(name) : 0f;
            return floatValue;
        }
        return base.GetValue(port);
    }
}
