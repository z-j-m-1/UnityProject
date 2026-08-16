#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

/// <summary>
/// 外部参数节点体编辑器：
/// - paramName 提供「图中其它参数节点已用参数名」下拉（保持一致、避免拼错；仍可手动输入，需与外部脚本键一致）；
/// - 运行时显示当前注入值（外部触发带参后可见，调试用）；物体类型隐藏 fallback（防误拖场景引用）。
/// </summary>
[CustomNodeEditor(typeof(ExternalParamNodeBase))]
public class ExternalParamNodeBodyEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();
        string[] excludes = { "m_Script", "graph", "position", "ports" };
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (System.Array.IndexOf(excludes, iterator.name) >= 0) continue;
            if (iterator.name == "paramName")
            {
                DrawParamNamePicker(iterator);
                continue;
            }
            // 物体类型的 fallback 留空即可，隐藏避免误拖场景引用
            if (iterator.name == "fallback" && target is ExternalParamGameObjectNode) continue;
            NodeEditorGUILayout.PropertyField(iterator, true);
        }
        foreach (NodePort dynamicPort in target.DynamicPorts)
        {
            if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
            NodeEditorGUILayout.PortField(dynamicPort);
        }
        serializedObject.ApplyModifiedProperties();

        // 运行时显示当前注入值（外部触发带参后才可见）
        if (Application.isPlaying && target is ExternalParamNodeBase ep && target.graph is BaseNodeGraph graph)
        {
            bool has = graph.TryGetExternalParam(ep.paramName, out object value);
            string display = has ? (value != null ? value.ToString() : "(null)") : "(未注入，返回默认值)";
            EditorGUILayout.LabelField("当前值", display);
        }
    }

    /// <summary>参数名下拉：收集图中所有外部参数节点已用的参数名，未命中保留手动输入</summary>
    private void DrawParamNamePicker(SerializedProperty prop)
    {
        BaseNode baseNode = target as BaseNode;
        NodeGraph graph = baseNode != null ? baseNode.graph : null;
        if (graph != null)
        {
            List<string> names = new List<string>();
            foreach (Node node in graph.nodes)
            {
                if (node is ExternalParamNodeBase ep && !string.IsNullOrEmpty(ep.paramName) && !names.Contains(ep.paramName))
                {
                    names.Add(ep.paramName);
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