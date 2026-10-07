using UnityEngine;
using XNode;

/// <summary>
/// 材质-设置属性：通过 <see cref="MaterialPropertyBlock"/> 写入材质属性。
/// **不改材质资产、不实例化材质副本、每个物体独立** —— 运行时改材质参数的推荐做法
/// （对比 renderer.material 会实例化泄漏、renderer.sharedMaterial 会改到共享资产/在编辑器里脏化 .mat）。
///
/// 值用「类型枚举 + 值联合」（TypedValue），一个节点覆盖 浮点 / 整数 / 颜色 / 向量 / 贴图
/// （MaterialPropertyBlock 能写的类型；布尔/字符串/物体等会警告并跳过）。
/// 属性名是输入端口（可接线）。
///
/// ⚠️ 写入前先 GetPropertyBlock 取回已有覆盖，避免清掉别处（其它节点/系统）写在同一渲染器上的属性。
/// </summary>
[CreateNodeMenu("材质/设置属性")]
[NodeWidth(280)]
public class SetMaterialPropertyNode : FlowNode
{
    [Header("目标物体（接线：任一「取值/获取物体」节点）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject targetGameObject;

    [Header("属性名（如 _BaseColor / _Dissolve）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string propertyName;

    [Header("值")]
    public TypedValue value = new TypedValue();

    // 复用同一份，避免每次执行 new 出 GC 垃圾
    private MaterialPropertyBlock block;

    public override void Execute()
    {
        GameObject obj = GetInputValue<GameObject>(nameof(targetGameObject), null);
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把任一「取值/获取物体」节点接到目标端口）");
            return;
        }

        Renderer targetRenderer = obj.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 上没有 Renderer");
            return;
        }

        string name = GetInputValue<string>(nameof(propertyName), propertyName);
        if (string.IsNullOrEmpty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 属性名为空");
            return;
        }

        Material sharedMaterial = targetRenderer.sharedMaterial;
        if (sharedMaterial != null && !sharedMaterial.HasProperty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 材质 '{sharedMaterial.name}' 上没有属性 '{name}'（检查拼写）");
            return;
        }

        if (block == null) block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);   // ★ 先取已有覆盖，避免清掉别处写过的属性

        if (!Apply(block, name))
        {
            NodeLog.Warning($"{GetType().Name}: 材质属性不支持类型 {value.TypeName}（PropertyBlock 只支持 浮点/整数/颜色/向量/贴图）");
            return;
        }

        targetRenderer.SetPropertyBlock(block);
        NodeLog.Info($"{GetType().Name}: 已设置 '{obj.name}'.{name} = {value.GetValue()}（{value.TypeName}）");
    }

    /// <summary>把值写进属性块；类型不支持返回 false</summary>
    private bool Apply(MaterialPropertyBlock target, string name)
    {
        switch (value.type)
        {
            case TypedValueType.Float: target.SetFloat(name, value.floatValue); return true;
            case TypedValueType.Int: target.SetInt(name, value.intValue); return true;
            case TypedValueType.Color: target.SetColor(name, value.colorValue); return true;
            case TypedValueType.Vector2: target.SetVector(name, value.vector2Value); return true;
            case TypedValueType.Vector3: target.SetVector(name, value.vector3Value); return true;
            case TypedValueType.Texture: target.SetTexture(name, value.textureValue); return true;
            default: return false;
        }
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
