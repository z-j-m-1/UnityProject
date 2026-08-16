#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

/// <summary>
/// 运行时节点高亮：Play 模式下扫描所有 GraphExecutor 的当前执行节点，编辑器里画亮边
/// 多执行器（同图 start / 多个入口同时跑）可同时高亮多个节点
/// </summary>
public static class NodeRunHighlight
{
    /// <summary>亮边颜色</summary>
    private static readonly Color HighlightColor = new Color(0.2f, 0.85f, 1f);

    /// <summary>当前所有执行器正在执行的节点集合</summary>
    public static readonly HashSet<BaseNode> ActiveNodes = new HashSet<BaseNode>();

    private static readonly HashSet<BaseNode> previousNodes = new HashSet<BaseNode>();

    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        ActiveNodes.Clear();

        // 仅 Play 模式扫描；用 FindObjectOfType 避免 Instance 单例在编辑模式被创建
        if (Application.isPlaying)
        {
            GraphCommunicator comm = Object.FindObjectOfType<GraphCommunicator>();
            if (comm != null)
            {
                foreach (var kvp in comm.GetAllExecutors())
                {
                    GraphExecutor executor = kvp.Value;
                    if (executor == null) continue;
                    foreach (BaseNode node in executor.RunningNodes)
                    {
                        if (node != null)
                        {
                            ActiveNodes.Add(node);
                        }
                    }
                }
            }
        }

        if (!previousNodes.SetEquals(ActiveNodes))
        {
            previousNodes.Clear();
            previousNodes.UnionWith(ActiveNodes);
            if (NodeEditorWindow.current != null)
            {
                NodeEditorWindow.current.Repaint();
            }
        }
    }

    public static bool IsActive(BaseNode node)
    {
        return node != null && ActiveNodes.Contains(node);
    }

    /// <summary>
    /// 若节点正在执行：画亮边背景并返回 true（配合 EndHighlight 使用）
    /// </summary>
    public static bool BeginIfActive(BaseNode node)
    {
        if (!IsActive(node)) return false;

        GUIStyle highlight = new GUIStyle(NodeEditorResources.styles.nodeHighlight);
        highlight.padding = new GUIStyle(NodeEditorResources.styles.nodeBody).padding;
        GUI.color = HighlightColor;
        GUILayout.BeginVertical(highlight);
        GUI.color = Color.white;
        return true;
    }

    public static void EndHighlight()
    {
        GUILayout.EndVertical();
    }
}
#endif
