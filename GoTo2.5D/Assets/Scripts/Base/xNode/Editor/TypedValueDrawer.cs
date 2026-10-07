#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// TypedValue 属性绘制器：两行布局 ——
///   第一行：字段标签 + 类型下拉
///   第二行：「值」标签 + 当前类型对应的值字段（其余类型的值字段隐藏）
///
/// 为什么在节点体里也能用：xNode 的 NodeEditorGUILayout.PropertyField 对**非端口**属性
/// 直接调用 EditorGUILayout.PropertyField（见其第 40 行），因此自定义绘制器与
/// GetPropertyHeight 的多行高度都会被正常应用。
/// </summary>
[CustomPropertyDrawer(typeof(TypedValue))]
public class TypedValueDrawer : PropertyDrawer
{
    private const float Gap = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty typeProp = property.FindPropertyRelative(nameof(TypedValue.type));

        float line = EditorGUIUtility.singleLineHeight;

        // 节点体很窄，标签宽度按可用宽度收窄，避免把下拉/值字段挤没
        float labelW = Mathf.Min(EditorGUIUtility.labelWidth, position.width * 0.45f);
        float fieldW = position.width - labelW;

        // ---- 第一行：标签 + 类型 ----
        Rect labelRect = new Rect(position.x, position.y, labelW, line);
        Rect typeRect = new Rect(position.x + labelW, position.y, fieldW, line);
        EditorGUI.LabelField(labelRect, label);
        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

        // ---- 第二行：值 ----
        float y2 = position.y + line + Gap;
        Rect valueLabelRect = new Rect(position.x, y2, labelW, line);
        Rect valueRect = new Rect(position.x + labelW, y2, fieldW, line);

        TypedValueType type = (TypedValueType)typeProp.enumValueIndex;
        string fieldName = TypedValue.FieldNameOf(type);

        if (string.IsNullOrEmpty(fieldName))
        {
            EditorGUI.LabelField(valueLabelRect, "值");
            EditorGUI.LabelField(valueRect, "（该类型无值）");
        }
        else
        {
            SerializedProperty fieldProp = property.FindPropertyRelative(fieldName);
            EditorGUI.LabelField(valueLabelRect, "值");
            if (fieldProp != null)
            {
                EditorGUI.PropertyField(valueRect, fieldProp, GUIContent.none);
            }
            else
            {
                EditorGUI.LabelField(valueRect, "(找不到字段 " + fieldName + ")");
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2f + Gap;
    }
}
#endif
