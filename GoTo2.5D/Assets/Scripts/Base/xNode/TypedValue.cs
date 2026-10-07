using System;
using UnityEngine;

/// <summary>「类型枚举 + 值联合」支持的类型</summary>
public enum TypedValueType
{
    /// <summary>无值（例：Animator 的 Trigger —— 只有名字没有值）</summary>
    None,
    Bool,
    Int,
    Float,
    String,
    Vector2,
    Vector3,
    Color,
    /// <summary>GameObject（⚠️ 节点字段受 Unity 限制只能放资产 / 预制体，场景物体拖不进来）</summary>
    GameObject,
    Sprite,
    AudioClip,
    Texture
}

/// <summary>
/// 可序列化的「类型枚举 + 值联合」：只使用与 <see cref="type"/> 对应的那个字段，其余在面板隐藏。
/// 由 <c>TypedValueDrawer</c> 绘制成「类型下拉 + 按类型显示的值字段」两行。
///
/// 用途：把"每种类型一个节点"合并成一个节点 ——
/// 例：材质属性设置（浮点/整数/颜色/向量/贴图）、Animator 参数（浮点/整数/布尔/Trigger）。
///
/// 实现要点：值字段全部 <see cref="HideInInspector"/>，所以默认迭代不会重复绘制它们；
/// 下拉切换类型后，由绘制器用 FindPropertyRelative 单独取出对应字段画出来
/// （HideInInspector 只影响可见性迭代，不影响 FindPropertyRelative 查找）。
/// </summary>
[Serializable]
public class TypedValue
{
    [Tooltip("值类型（切换后下方值字段随之变化）")]
    public TypedValueType type = TypedValueType.Float;

    [HideInInspector] public bool boolValue;
    [HideInInspector] public int intValue;
    [HideInInspector] public float floatValue;
    [HideInInspector] public string stringValue;
    [HideInInspector] public Vector2 vector2Value;
    [HideInInspector] public Vector3 vector3Value;
    [HideInInspector] public Color colorValue = Color.white;
    [HideInInspector] public GameObject gameObjectValue;
    [HideInInspector] public Sprite spriteValue;
    [HideInInspector] public AudioClip audioClipValue;
    [HideInInspector] public Texture textureValue;

    /// <summary>类型 → 对应字段名（None 返回 null，表示该类型没有值字段）</summary>
    public static string FieldNameOf(TypedValueType type)
    {
        switch (type)
        {
            case TypedValueType.Bool: return nameof(boolValue);
            case TypedValueType.Int: return nameof(intValue);
            case TypedValueType.Float: return nameof(floatValue);
            case TypedValueType.String: return nameof(stringValue);
            case TypedValueType.Vector2: return nameof(vector2Value);
            case TypedValueType.Vector3: return nameof(vector3Value);
            case TypedValueType.Color: return nameof(colorValue);
            case TypedValueType.GameObject: return nameof(gameObjectValue);
            case TypedValueType.Sprite: return nameof(spriteValue);
            case TypedValueType.AudioClip: return nameof(audioClipValue);
            case TypedValueType.Texture: return nameof(textureValue);
            default: return null;
        }
    }

    /// <summary>当前类型对应的运行期值（None 返回 null）</summary>
    public object GetValue()
    {
        switch (type)
        {
            case TypedValueType.Bool: return boolValue;
            case TypedValueType.Int: return intValue;
            case TypedValueType.Float: return floatValue;
            case TypedValueType.String: return stringValue;
            case TypedValueType.Vector2: return vector2Value;
            case TypedValueType.Vector3: return vector3Value;
            case TypedValueType.Color: return colorValue;
            case TypedValueType.GameObject: return gameObjectValue;
            case TypedValueType.Sprite: return spriteValue;
            case TypedValueType.AudioClip: return audioClipValue;
            case TypedValueType.Texture: return textureValue;
            default: return null;
        }
    }

    /// <summary>当前类型的面板短名（日志 / 提示用）</summary>
    public string TypeName => type == TypedValueType.None ? "无值" : type.ToString();
}
