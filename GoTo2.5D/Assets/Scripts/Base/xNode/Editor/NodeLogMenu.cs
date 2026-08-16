#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>节点系统日志级别快捷切换（Tools/节点系统/日志级别）</summary>
public static class NodeLogMenu
{
    [MenuItem("Tools/节点系统/日志级别/警告（默认）")]
    public static void SetWarning() => Set(LogLevel.Warning);

    [MenuItem("Tools/节点系统/日志级别/信息")]
    public static void SetInfo() => Set(LogLevel.Info);

    [MenuItem("Tools/节点系统/日志级别/详细")]
    public static void SetVerbose() => Set(LogLevel.Verbose);

    private static void Set(LogLevel level)
    {
        NodeLog.Level = level;
        Debug.Log($"NodeLog 日志级别: {level}");
    }

    [MenuItem("Tools/节点系统/日志级别/警告（默认）", true)]
    public static bool VWarning() { Menu.SetChecked("Tools/节点系统/日志级别/警告（默认）", NodeLog.Level == LogLevel.Warning); return true; }

    [MenuItem("Tools/节点系统/日志级别/信息", true)]
    public static bool VInfo() { Menu.SetChecked("Tools/节点系统/日志级别/信息", NodeLog.Level == LogLevel.Info); return true; }

    [MenuItem("Tools/节点系统/日志级别/详细", true)]
    public static bool VVerbose() { Menu.SetChecked("Tools/节点系统/日志级别/详细", NodeLog.Level == LogLevel.Verbose); return true; }
}
#endif
