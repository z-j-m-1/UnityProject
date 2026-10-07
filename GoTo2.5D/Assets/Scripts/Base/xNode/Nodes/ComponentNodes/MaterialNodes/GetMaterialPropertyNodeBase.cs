using UnityEngine;
using XNode;

/// <summary>
/// 材质属性读取节点基类（数据节点）：统一「目标物体端口 + 解析出 Material 与属性名」，
/// 子类各自重写 GetValue 读浮点 / 颜色。
///
/// 读的是 <c>renderer.sharedMaterial</c>（**材质资产上的值**），
/// **不含**本图通过 MaterialPropertyBlock 写入的覆盖值 —— 所以它正好适合
/// "先记录原始值，之后渐变回原样"这类用法。
/// </summary>
public abstract class GetMaterialPropertyNodeBase : DataNode
{
    [Header("目标物体（接线：任一「取值/获取物体」节点）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject targetGameObject;

    [Header("属性名（如 _BaseColor / _Dissolve）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string propertyName;

    /// <summary>解析目标材质与属性名；任何一步失败都警告并返回 null</summary>
    protected Material ResolveMaterial(out string name)
    {
        name = null;

        GameObject obj = GetInputValue<GameObject>(nameof(targetGameObject), null);
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把任一「取值/获取物体」节点接到目标端口）");
            return null;
        }

        Renderer targetRenderer = obj.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 上没有 Renderer");
            return null;
        }

        name = GetInputValue<string>(nameof(propertyName), propertyName);
        if (string.IsNullOrEmpty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 属性名为空");
            return null;
        }

        Material material = targetRenderer.sharedMaterial;
        if (material == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 的渲染器没有材质");
            return null;
        }

        if (!material.HasProperty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 材质 '{material.name}' 上没有属性 '{name}'（检查拼写）");
            return null;
        }

        return material;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(propertyName))
        {
            return GetInputValue<string>(nameof(propertyName), propertyName);
        }
        return null;
    }
}
