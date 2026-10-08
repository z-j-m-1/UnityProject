#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// GraphParamEntry 属性绘制器：一行「名称 + 类型下拉」，下一行「按类型显示的值字段」（其余类型字段隐藏）。
/// 让外部脚本面板上的参数可视化编辑：增删参数、切换类型即换值字段、空名提示。
///
/// ⚠️ 值区域高度是**动态**的：早期版本把整块高度写死成两行，曲线（AnimationCurve）编辑器比一行高得多，
/// 会被直接裁掉。现在用 <see cref="EditorGUI.GetPropertyHeight"/> 按实际字段求高，
/// 以后再加"高字段"类型（曲线 / 对象列表等）也不会出问题。
/// </summary>
[CustomPropertyDrawer(typeof(GraphParamEntry))]
public class GraphParamEntryDrawer : PropertyDrawer
{
    private const float Gap = 2f;
    private const float Padding = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty nameProp = property.FindPropertyRelative("name");
        SerializedProperty typeProp = property.FindPropertyRelative("type");
        SerializedProperty valueProp = property.FindPropertyRelative("value");

        float line = EditorGUIUtility.singleLineHeight;

        Rect nameRect = new Rect(position.x, position.y, position.width * 0.60f, line);
        Rect typeRect = new Rect(position.x + position.width * 0.64f, position.y, position.width * 0.36f, line);
        EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

        Rect valueRect = new Rect(position.x, position.y + line + Gap, position.width, ValueHeight(typeProp, valueProp));

        if (string.IsNullOrEmpty(nameProp.stringValue))
        {
            EditorGUI.HelpBox(valueRect, "参数名为空，Build 时跳过", MessageType.Warning);
        }
        else
        {
            SerializedProperty fieldProp = FindValueField(typeProp, valueProp);
            if (fieldProp != null)
            {
                EditorGUI.PropertyField(valueRect, fieldProp, new GUIContent("值"));
            }
            else
            {
                EditorGUI.LabelField(valueRect, "值", "(无)");
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty nameProp = property.FindPropertyRelative("name");
        SerializedProperty typeProp = property.FindPropertyRelative("type");
        SerializedProperty valueProp = property.FindPropertyRelative("value");

        // 名称/类型一行 + 值区域（高度随类型变化）
        return EditorGUIUtility.singleLineHeight + Gap + ValueHeight(typeProp, valueProp) + Padding;
    }

    /// <summary>值区域高度：按当前类型对应字段的实际高度算（曲线等比一行高得多）</summary>
    private float ValueHeight(SerializedProperty typeProp, SerializedProperty valueProp)
    {
        float line = EditorGUIUtility.singleLineHeight;

        SerializedProperty fieldProp = FindValueField(typeProp, valueProp);
        if (fieldProp == null) return line;

        return Mathf.Max(EditorGUI.GetPropertyHeight(fieldProp, new GUIContent("值"), true), line);
    }

    private SerializedProperty FindValueField(SerializedProperty typeProp, SerializedProperty valueProp)
    {
        if (typeProp == null || valueProp == null) return null;

        GraphParamType type = (GraphParamType)typeProp.enumValueIndex;
        return valueProp.FindPropertyRelative(GraphParamValue.FieldNameOf(type));
    }
}
#endif
