#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

/// <summary>
/// SubGraphNode 节点体编辑器：
/// 子图资产下拉（排除自身所在图）、入口下拉、循环引用与参数重名校验
/// </summary>
[CustomNodeEditor(typeof(SubGraphNode))]
public class SubGraphNodeBodyEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        BaseNode baseNode = target as BaseNode;
        bool highlighted = NodeRunHighlight.BeginIfActive(baseNode);

        serializedObject.Update();
        SerializedProperty subGraphProp = serializedObject.FindProperty("subGraph");
        SerializedProperty entryProp = serializedObject.FindProperty("entryIdentifier");

        DrawSubGraphPicker(subGraphProp);
        DrawEntryPicker(subGraphProp, entryProp);
        DrawValidations(subGraphProp);

        string[] excludes = { "m_Script", "graph", "position", "ports", "subGraph", "entryIdentifier" };
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (System.Array.IndexOf(excludes, iterator.name) >= 0) continue;
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

    private void DrawSubGraphPicker(SerializedProperty subGraphProp)
    {
        string[] guids = AssetDatabase.FindAssets("t:BaseNodeGraph");
        List<string> labels = new List<string>();
        List<string> validGuids = new List<string>();
        string selfPath = AssetDatabase.GetAssetPath((target as XNode.Node).graph);
        string currentGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(subGraphProp.objectReferenceValue));
        int current = -1;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path == selfPath) continue; // 排除自身所在图
            labels.Add(path);
            validGuids.Add(guid);
            if (guid == currentGuid) current = labels.Count - 1;
        }

        if (labels.Count == 0)
        {
            EditorGUILayout.HelpBox("项目中除本图外没有其他节点图资产", MessageType.Info);
        }
        else
        {
            string[] display = new string[labels.Count + 1];
            display[0] = "（选择子图…）";
            labels.CopyTo(display, 1);
            int sel = current >= 0 ? current + 1 : 0;
            int picked = EditorGUILayout.Popup("子图资产", sel, display);
            if (picked > 0 && (current < 0 || picked - 1 != current))
            {
                subGraphProp.objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<BaseNodeGraph>(AssetDatabase.GUIDToAssetPath(validGuids[picked - 1]));
            }
        }

        // 仍允许直接拖引用
        EditorGUILayout.PropertyField(subGraphProp, new GUIContent("子图引用"));
    }

    private void DrawEntryPicker(SerializedProperty subGraphProp, SerializedProperty entryProp)
    {
        BaseNodeGraph graph = subGraphProp.objectReferenceValue as BaseNodeGraph;
        string current = entryProp.stringValue ?? "";
        if (graph == null)
        {
            EditorGUILayout.PropertyField(entryProp);
            return;
        }

        List<EntryNode> entries = graph.GetAllEntryNodes();
        string[] options = new string[entries.Count + 1];
        options[0] = "（默认起点）";
        int sel = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            string id = entries[i].Identifier;
            options[i + 1] = string.IsNullOrEmpty(id) ? "(未命名入口)" : id;
            if (id == current) sel = i + 1;
        }

        int picked = EditorGUILayout.Popup("入口", sel, options);
        if (picked == 0)
        {
            if (sel != 0) entryProp.stringValue = "";
        }
        else if (picked != sel)
        {
            entryProp.stringValue = entries[picked - 1].Identifier;
        }
        EditorGUILayout.PropertyField(entryProp);
    }

    private void DrawValidations(SerializedProperty subGraphProp)
    {
        BaseNodeGraph graph = subGraphProp.objectReferenceValue as BaseNodeGraph;
        if (graph == null) return;

        if (ContainsCycle(graph, new HashSet<BaseNodeGraph>()))
        {
            EditorGUILayout.HelpBox("存在循环引用（子图直接或间接包含自身），运行时会被深度上限拦截", MessageType.Error);
        }

        HashSet<string> names = new HashSet<string>();
        foreach (XNode.Node n in graph.nodes)
        {
            string name = null;
            if (n is SubGraphInputNodeBase input) name = input.parameterName;
            else if (n is SubGraphOutputNodeBase output) name = output.parameterName;
            if (string.IsNullOrEmpty(name)) continue;
            if (!names.Add(name))
            {
                EditorGUILayout.HelpBox($"子图参数名重复：'{name}'（同名参数只保留第一个端口）", MessageType.Warning);
            }
        }
    }

    /// <summary>路径回溯检测循环引用（允许菱形引用）</summary>
    private bool ContainsCycle(BaseNodeGraph graph, HashSet<BaseNodeGraph> onPath)
    {
        if (graph == null) return false;
        if (onPath.Contains(graph)) return true;
        onPath.Add(graph);
        foreach (XNode.Node n in graph.nodes)
        {
            if (n is SubGraphNode sg && ContainsCycle(sg.subGraph, onPath)) return true;
        }
        onPath.Remove(graph);
        return false;
    }
}
#endif
