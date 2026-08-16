#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

/// <summary>
/// 参数输入节点（统一「参数/输入」）体编辑器：
/// - parameterName 提供「图中其它参数节点已用参数名」下拉（保持一致、避免拼错；仍可手动输入，需与调用方键一致）；
/// - 运行时显示当前注入值（子图/外部/事件/状态机任一调用方注入后可见，调试用）；
/// - 非序列化端口（物体类型 value）由 VisiblePortsNodeEditor 补画。
/// </summary>
[CustomNodeEditor(typeof(SubGraphInputNodeBase))]
public class SubGraphInputNodeBodyEditor : VisiblePortsNodeEditor
{
    protected override bool OnDrawProperty(SerializedProperty property)
    {
        if (property.name == "parameterName")
        {
            DrawParameterNamePicker(property);
            return true;
        }
        return false;
    }

    protected override void OnBodyFooter()
    {
        if (Application.isPlaying && target is SubGraphInputNodeBase input && target.graph is BaseNodeGraph graph)
        {
            bool has = graph.TryGetInvocationParam(input.parameterName, out object value);
            string display = has ? (value != null ? value.ToString() : "(null)") : "(未注入，用默认值)";
            EditorGUILayout.LabelField("当前值", display);
        }
    }

    /// <summary>参数名下拉：收集图中所有参数输入节点已用的参数名，未命中保留手动输入</summary>
    private void DrawParameterNamePicker(SerializedProperty prop)
    {
        BaseNode baseNode = target as BaseNode;
        NodeGraph graph = baseNode != null ? baseNode.graph : null;
        if (graph != null)
        {
            List<string> names = new List<string>();
            foreach (Node node in graph.nodes)
            {
                if (node is SubGraphInputNodeBase input && !string.IsNullOrEmpty(input.parameterName) && !names.Contains(input.parameterName))
                {
                    names.Add(input.parameterName);
                }
            }
            if (names.Count > 0)
            {
                string current = prop.stringValue ?? "";
                int idx = names.IndexOf(current);
                if (idx >= 0)
                {
                    int newIdx = EditorGUILayout.Popup("参数名", idx, names.ToArray());
                    if (newIdx != idx)
                    {
                        prop.stringValue = names[newIdx];
                    }
                }
                else
                {
                    string[] opts = new string[names.Count + 1];
                    opts[0] = "（选择图中参数名…）";
                    for (int i = 0; i < names.Count; i++)
                    {
                        opts[i + 1] = names[i];
                    }
                    int pick = EditorGUILayout.Popup("参数名", 0, opts);
                    if (pick > 0)
                    {
                        prop.stringValue = names[pick - 1];
                    }
                }
            }
        }
        NodeEditorGUILayout.PropertyField(prop);
    }
}
#endif