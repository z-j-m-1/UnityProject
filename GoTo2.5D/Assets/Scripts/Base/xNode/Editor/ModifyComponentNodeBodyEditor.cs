#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

/// <summary>
/// 组件-添加·移除 的节点体编辑器：把「组件类型全名」做成**下拉选择**（避免手填拼错）。
///
/// 列表来自 <see cref="TypeCache"/>，只收**非 Unity 自带**程序集里的非抽象 MonoBehaviour ——
/// UnityEngine / UnityEditor 的组件都有专门的组件节点（如 SpriteRenderer / Rigidbody 各有对应节点），
/// 没必要在这里动态添加，也会把列表撑到几百项。
/// 下拉下方保留文本字段：可以手填短名或粘贴全名（运行期有短名兜底解析）。
/// </summary>
[CustomNodeEditor(typeof(ModifyComponentNode))]
public class ModifyComponentNodeBodyEditor : VisiblePortsNodeEditor
{
    private static List<string> cachedNames;

    protected override bool OnDrawProperty(SerializedProperty property)
    {
        if (property.name != nameof(ModifyComponentNode.componentTypeName)) return false;

        DrawTypePicker(property);
        return true;
    }

    private static void DrawTypePicker(SerializedProperty prop)
    {
        List<string> names = GetNames();
        string current = prop.stringValue ?? string.Empty;
        int idx = names.IndexOf(current);

        string[] options = new string[names.Count + 1];
        options[0] = names.Count > 0 ? "（选择组件类型…）" : "（没找到可用脚本组件）";
        for (int i = 0; i < names.Count; i++)
        {
            options[i + 1] = Shorten(names[i]);
        }

        int pick = EditorGUILayout.Popup("组件类型", idx >= 0 ? idx + 1 : 0, options);
        if (pick > 0 && pick - 1 != idx)
        {
            prop.stringValue = names[pick - 1];
        }

        // 下方保留原字段，便于手填/粘贴
        NodeEditorGUILayout.PropertyField(prop);
    }

    /// <summary>下拉里显示短名，但存的仍是全名（避免长列表难读）</summary>
    private static string Shorten(string fullName)
    {
        int lastDot = fullName.LastIndexOf('.');
        return lastDot >= 0 ? fullName.Substring(lastDot + 1) : fullName;
    }

    private static List<string> GetNames()
    {
        if (cachedNames != null) return cachedNames;

        cachedNames = new List<string>();

        foreach (Type t in TypeCache.GetTypesDerivedFrom<MonoBehaviour>())
        {
            if (t == null || t.IsAbstract || t.IsGenericTypeDefinition) continue;

            string asm = t.Assembly.GetName().Name;
            if (asm.StartsWith("Unity", StringComparison.Ordinal)) continue;   // 排除 Unity 自带模块

            cachedNames.Add(t.FullName);
        }

        cachedNames.Sort(StringComparer.Ordinal);
        return cachedNames;
    }
}
#endif
