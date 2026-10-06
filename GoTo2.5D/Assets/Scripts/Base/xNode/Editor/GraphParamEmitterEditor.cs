using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GraphParamEmitter 自定义 Inspector：复用 GraphEventEntryOptionPicker，从场景节点图/入口生成 eventId 下拉，
/// 与 GraphEventEmitterEditor / CollisionEventEmitterEditor 行为一致。
///
/// 注意：自定义 Inspector **接管整个面板**，不再自动绘制其余字段。
/// GraphParamEmitter 除 eventId 外还有 parameters（GraphParamList），所以这里必须自己把它画出来，
/// 否则参数包会在 Inspector 里消失（GraphEventEmitter / CollisionEventEmitter 只有 eventId，故无此问题）。
/// </summary>
[CustomEditor(typeof(GraphParamEmitter))]
public class GraphParamEmitterEditor : Editor
{
    private SerializedProperty eventIdProp;
    private SerializedProperty parametersProp;
    private List<GraphEventEntryOptionPicker.EntryOption> options = new List<GraphEventEntryOptionPicker.EntryOption>();
    private bool dirty = true;

    private void OnEnable()
    {
        eventIdProp = serializedObject.FindProperty("eventId");
        parametersProp = serializedObject.FindProperty("parameters");
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

        // 入口标识：下拉（命中即定位，未命中给占位）+ 下方手动输入框，均由选择器统一绘制
        GraphEventEntryOptionPicker.DrawEventIdPicker(eventIdProp, options);

        if (GUILayout.Button("刷新入口列表"))
        {
            options = GraphEventEntryOptionPicker.CollectEntryOptions();
            dirty = false;
        }

        EditorGUILayout.Space();

        // 参数包（GraphParamList）：Emit 时随事件作为 GraphEvent.data 注入图调用参数存储
        EditorGUILayout.PropertyField(parametersProp, new GUIContent("参数包"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
