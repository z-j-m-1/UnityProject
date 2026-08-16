using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 图事件入口选择器（编辑器静态帮助类）
/// 扫描场景中 GraphExecutor 使用的节点图及其入口节点，为 GraphEventEmitter / CollisionEventEmitter 等触发器
/// 生成 eventId 下拉选项，避免手填标识符拼写错误（错误会拖到运行时才报"未找到入口"）。
/// 后续其他触发器（按钮、对话、关卡事件等）只需在 CustomEditor 里调用 Collect + Draw 即可复用。
/// </summary>
public static class GraphEventEntryOptionPicker
{
    /// <summary>单个入口下拉选项：值 = 标识符（回填 eventId），显示 = 带图名前缀的文案</summary>
    public struct EntryOption
    {
        public string identifier;   // 回填到 eventId 的值（入口 Identifier，匹配时优先名字）
        public string label;        // 显示文案：标识符（图名·节点名）
        public BaseNodeGraph graph;
        public EntryNode entry;
    }

    /// <summary>扫描场景中所有执行器 → 按图去重 → 收集全部入口节点</summary>
    public static List<EntryOption> CollectEntryOptions()
    {
        List<EntryOption> options = new List<EntryOption>();
        List<BaseNodeGraph> seenGraphs = new List<BaseNodeGraph>();

        GraphExecutor[] executors = Object.FindObjectsByType<GraphExecutor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GraphExecutor executor in executors)
        {
            BaseNodeGraph graph = executor.GetNodeGraph();
            if (graph == null || seenGraphs.Contains(graph)) continue;
            seenGraphs.Add(graph);

            foreach (EntryNode entry in graph.GetAllEntryNodes())
            {
                string identifier = entry.Identifier ?? "";
                string idLabel = string.IsNullOrEmpty(identifier) ? "(未命名入口)" : identifier;
                options.Add(new EntryOption
                {
                    identifier = identifier,
                    label = $"{idLabel}（{graph.name}·{entry.name}）",
                    graph = graph,
                    entry = entry
                });
            }
        }
        return options;
    }

    /// <summary>
    /// 绘制 eventId 选择器：始终显示入口下拉（命中→定位选中；未命中→占位首项可选、不覆盖手填值），
    /// 空场景仅给提示；下方始终保留手动字段（eventId 也可能给非入口监听器使用）
    /// </summary>
    public static void DrawEventIdPicker(SerializedProperty eventIdProp, List<EntryOption> options)
    {
        string current = eventIdProp.stringValue ?? "";

        if (options.Count == 0)
        {
            EditorGUILayout.HelpBox("场景中未找到带入口节点的节点图执行器（GraphExecutor），可在下方手动输入，或挂好执行器后点击「刷新入口列表」", MessageType.Info);
        }
        else
        {
            string[] labels = new string[options.Count];
            int selectedIndex = -1;
            for (int i = 0; i < options.Count; i++)
            {
                labels[i] = options[i].label;
                if (selectedIndex < 0 && options[i].identifier == current)
                {
                    selectedIndex = i;
                }
            }

            if (selectedIndex >= 0)
            {
                // 已命中：下拉直接定位当前入口，选其他项即回填
                int newIndex = EditorGUILayout.Popup("入口标识", selectedIndex, labels);
                if (newIndex != selectedIndex)
                {
                    eventIdProp.stringValue = options[newIndex].identifier;
                }
            }
            else
            {
                // 未命中：下拉始终可见（首项为占位，不写入），选真实入口才回填；警告保留
                string[] pickLabels = new string[labels.Length + 1];
                pickLabels[0] = "（选择入口标识…）";
                for (int i = 0; i < labels.Length; i++)
                {
                    pickLabels[i + 1] = labels[i];
                }

                int newIndex = EditorGUILayout.Popup("入口标识", 0, pickLabels);
                if (newIndex > 0)
                {
                    eventIdProp.stringValue = options[newIndex - 1].identifier;
                }

                EditorGUILayout.HelpBox(
                    string.IsNullOrEmpty(current)
                        ? "当前 eventId 为空，请从上方下拉选择入口标识，或在下方手动输入"
                        : $"当前 eventId '{current}' 未匹配到场景中任一入口节点标识，请从上方下拉重新选择或修正输入",
                    MessageType.Warning);
            }
        }

        EditorGUILayout.PropertyField(eventIdProp);
    }
}
