#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

/// <summary>
/// Get/Set 变量节点 + 入口节点的节点体编辑器：
/// 在节点图视图中隐藏 variableGuid / guid（避免误触），检查器里仍正常显示
/// 变量名提供「图中已有变量」下拉（避免手填拼错）
/// </summary>
public abstract class VariableNodeBodyEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        BaseNode baseNode = target as BaseNode;
        bool highlighted = NodeRunHighlight.BeginIfActive(baseNode);

        serializedObject.Update();
        string[] excludes = { "m_Script", "graph", "position", "ports", "variableGuid", "guid" };
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (System.Array.IndexOf(excludes, iterator.name) >= 0) continue;
            if (iterator.name == "variableName")
            {
                DrawVariableNamePicker(iterator);
                continue;
            }
            NodeEditorGUILayout.PropertyField(iterator, true);
        }
        foreach (XNode.NodePort dynamicPort in target.DynamicPorts)
        {
            if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
            NodeEditorGUILayout.PortField(dynamicPort);
        }
        serializedObject.ApplyModifiedProperties();

        if (highlighted)
        {
            NodeRunHighlight.EndHighlight();
        }
    }

    /// <summary>变量名下拉：从当前图已定义变量里选（未命中时首项为占位，选中才回填）；下方保留端口/字段</summary>
    private void DrawVariableNamePicker(SerializedProperty prop)
    {
        BaseNode baseNode = target as BaseNode;
        BaseNodeGraph graph = baseNode != null ? baseNode.graph as BaseNodeGraph : null;
        if (graph != null)
        {
            List<string> names = graph.GetAllVariableNames();
            if (names.Count > 0)
            {
                string current = prop.stringValue ?? "";
                int idx = names.IndexOf(current);
                if (idx >= 0)
                {
                    int newIdx = EditorGUILayout.Popup("变量", idx, names.ToArray());
                    if (newIdx != idx)
                    {
                        prop.stringValue = names[newIdx];
                    }
                }
                else
                {
                    string[] opts = new string[names.Count + 1];
                    opts[0] = "（选择图中变量…）";
                    for (int i = 0; i < names.Count; i++)
                    {
                        opts[i + 1] = names[i];
                    }
                    int pick = EditorGUILayout.Popup("变量", 0, opts);
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

/// <summary>获取变量节点：节点体隐藏 variableGuid</summary>
[CustomNodeEditor(typeof(GetVariableNodeBase))]
public class GetVariableNodeBodyEditor : VariableNodeBodyEditor
{
}

/// <summary>设置变量节点：节点体隐藏 variableGuid</summary>
[CustomNodeEditor(typeof(SetVariableNodeBase))]
public class SetVariableNodeBodyEditor : VariableNodeBodyEditor
{
}

/// <summary>入口节点：节点体隐藏 guid</summary>
[CustomNodeEditor(typeof(EntryNode))]
public class EntryNodeBodyEditor : VariableNodeBodyEditor
{
}
#endif
