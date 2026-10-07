using UnityEngine;
using XNode;

/// <summary>材质-读取颜色属性：输出 sharedMaterial 上某个颜色属性（如 _BaseColor / _EmissionColor）。</summary>
[CreateNodeMenu("材质/读取颜色")]
public class GetMaterialColorNode : GetMaterialPropertyNodeBase
{
    [Header("颜色值")]
    [Output]
    public Color colorValue = Color.white;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(colorValue))
        {
            string name;
            Material material = ResolveMaterial(out name);
            colorValue = material != null ? material.GetColor(name) : Color.white;
            return colorValue;
        }
        return base.GetValue(port);
    }
}
