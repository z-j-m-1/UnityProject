#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// GraphParamEntry 属性绘制器：一行「名称 + 类型下拉」，下一行「按类型显示的值字段」（其余类型字段隐藏）。
/// 让外部脚本面板上的参数可视化编辑：增删参数、切换类型即换值字段、空名提示。
/// </summary>
[CustomPropertyDrawer(typeof(GraphParamEntry))]
public class GraphParamEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty nameProp = property.FindPropertyRelative("name");
        SerializedProperty typeProp = property.FindPropertyRelative("type");
        SerializedProperty valueProp = property.FindPropertyRelative("value");

        float line = EditorGUIUtility.singleLineHeight;
        float gap = 2f;

        Rect nameRect = new Rect(position.x, position.y, position.width * 0.60f, line);
        Rect typeRect = new Rect(position.x + position.width * 0.64f, position.y, position.width * 0.36f, line);
        EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

        bool emptyName = string.IsNullOrEmpty(nameProp.stringValue);
        Rect valueRect = new Rect(position.x, position.y + line + gap, position.width, line);
        if (emptyName)
        {
            EditorGUI.HelpBox(valueRect, "参数名为空，Build 时跳过", MessageType.Warning);
        }
        else
        {
            GraphParamType type = (GraphParamType)typeProp.enumValueIndex;
            SerializedProperty fieldProp = valueProp.FindPropertyRelative(FieldName(type));
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
        bool emptyName = nameProp != null && string.IsNullOrEmpty(nameProp.stringValue);
        return EditorGUIUtility.singleLineHeight * 2f + 4f;
    }

    private string FieldName(GraphParamType type)
    {
        switch (type)
        {
            case GraphParamType.String: return "stringValue";
            case GraphParamType.Bool: return "boolValue";
            case GraphParamType.Int: return "intValue";
            case GraphParamType.Float: return "floatValue";
            case GraphParamType.Vector2: return "vector2Value";
            case GraphParamType.Vector3: return "vector3Value";
            case GraphParamType.GameObject: return "objectValue";
            default: return "floatValue";
        }
    }
}
#endif