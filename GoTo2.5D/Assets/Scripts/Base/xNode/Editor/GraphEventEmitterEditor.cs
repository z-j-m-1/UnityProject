using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GraphEventEmitter 自定义 Inspector：从场景节点图/入口生成 eventId 下拉，避免手填标识符拼错
/// </summary>
[CustomEditor(typeof(GraphEventEmitter))]
public class GraphEventEmitterEditor : Editor
{
    private SerializedProperty eventIdProp;
    private List<GraphEventEntryOptionPicker.EntryOption> options = new List<GraphEventEntryOptionPicker.EntryOption>();
    private bool dirty = true;

    private void OnEnable()
    {
        eventIdProp = serializedObject.FindProperty("eventId");
        MarkDirty();

        EditorSceneManager.sceneOpened += OnSceneChanged;
        EditorApplication.hierarchyChanged += MarkDirty;
        EditorApplication.projectChanged += MarkDirty;
    }

    private void OnDisable()
    {
        EditorSceneManager.sceneOpened -= OnSceneChanged;
        EditorApplication.hierarchyChanged -= MarkDirty;
        EditorApplication.projectChanged -= MarkDirty;
    }

    private void OnSceneChanged(Scene scene, OpenSceneMode mode) => MarkDirty();

    private void MarkDirty()
    {
        dirty = true;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (dirty)
        {
            options = GraphEventEntryOptionPicker.CollectEntryOptions();
            dirty = false;
        }

        GraphEventEntryOptionPicker.DrawEventIdPicker(eventIdProp, options);

        if (GUILayout.Button("刷新入口列表"))
        {
            options = GraphEventEntryOptionPicker.CollectEntryOptions();
            dirty = false;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
