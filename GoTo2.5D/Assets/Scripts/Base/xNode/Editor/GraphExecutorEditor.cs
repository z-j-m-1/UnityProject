#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// GraphExecutor 自定义 Inspector：
/// - 入口执行模式下显示入口节点下拉（从节点图实时扫描）
/// - 编辑模式下按 GUID 命中但标识符已改名时自动回填新标识符（自限一次）
/// </summary>
[CustomEditor(typeof(GraphExecutor))]
public class GraphExecutorEditor : Editor
{
    private SerializedProperty nodeGraphProp;
    private SerializedProperty autoExecuteProp;
    private SerializedProperty executeIntervalProp;
    private SerializedProperty executeCountProp;
    private SerializedProperty executionModeProp;
    private SerializedProperty entryIdentifierProp;

    private void OnEnable()
    {
        nodeGraphProp = serializedObject.FindProperty("nodeGraph");
        autoExecuteProp = serializedObject.FindProperty("autoExecute");
        executeIntervalProp = serializedObject.FindProperty("executeInterval");
        executeCountProp = serializedObject.FindProperty("executeCount");
        executionModeProp = serializedObject.FindProperty("executionMode");
        entryIdentifierProp = serializedObject.FindProperty("entryIdentifier");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(nodeGraphProp);
        EditorGUILayout.PropertyField(autoExecuteProp);
        EditorGUILayout.PropertyField(executeIntervalProp);
        EditorGUILayout.PropertyField(executeCountProp);
        EditorGUILayout.PropertyField(executionModeProp);

        if (executionModeProp.enumValueIndex == (int)GraphExecutionMode.Entry)
        {
            DrawEntrySelector();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawEntrySelector()
    {
        BaseNodeGraph graph = nodeGraphProp.objectReferenceValue as BaseNodeGraph;
        string currentId = entryIdentifierProp.stringValue;

        if (graph == null)
        {
            EditorGUILayout.HelpBox("请先指定节点图（nodeGraph）", MessageType.Warning);
            EditorGUILayout.PropertyField(entryIdentifierProp);
            return;
        }

        // 编辑模式自动修正：按 GUID 命中但标识符已改名 → 回填新标识符（自限一次，改完不再有差异）
        if (!Application.isPlaying && !string.IsNullOrEmpty(currentId))
        {
            EntryNode matched = graph.GetEntryNode(currentId);
            if (matched != null && matched.Identifier != currentId)
            {
                entryIdentifierProp.stringValue = matched.Identifier;
                MarkSceneDirty();
                currentId = matched.Identifier;
                EditorGUILayout.HelpBox($"入口标识符已自动更新为 '{matched.Identifier}'（原标识符已改名）", MessageType.Info);
            }
        }

        System.Collections.Generic.List<EntryNode> entries = graph.GetAllEntryNodes();
        if (entries.Count == 0)
        {
            EditorGUILayout.HelpBox("节点图中没有入口节点，请添加「基本/入口」节点", MessageType.Warning);
            EditorGUILayout.PropertyField(entryIdentifierProp);
            return;
        }

        string[] options = new string[entries.Count];
        int selectedIndex = -1;
        for (int i = 0; i < entries.Count; i++)
        {
            options[i] = string.IsNullOrEmpty(entries[i].Identifier) ? "(未命名入口)" : entries[i].Identifier;
            if (selectedIndex < 0 && entries[i].Identifier == currentId)
            {
                selectedIndex = i;
            }
        }

        if (selectedIndex >= 0)
        {
            int newIndex = EditorGUILayout.Popup("入口节点", selectedIndex, options);
            if (newIndex != selectedIndex)
            {
                entryIdentifierProp.stringValue = entries[newIndex].Identifier;
                MarkSceneDirty();
            }
        }
        else
        {
            EditorGUILayout.HelpBox($"当前标识符 '{currentId}' 未匹配到图中的入口节点", MessageType.Warning);
            if (!Application.isPlaying)
            {
                int newIndex = EditorGUILayout.Popup("入口节点", 0, options);
                entryIdentifierProp.stringValue = entries[newIndex].Identifier;
                MarkSceneDirty();
            }
            EditorGUILayout.PropertyField(entryIdentifierProp);
            return;
        }

        EditorGUILayout.PropertyField(entryIdentifierProp);
    }

    private void MarkSceneDirty()
    {
        if (Application.isPlaying) return;
        if (serializedObject.targetObject is MonoBehaviour mb)
        {
            EditorSceneManager.MarkSceneDirty(mb.gameObject.scene);
        }
    }
}
#endif
