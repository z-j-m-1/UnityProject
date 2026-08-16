using UnityEngine;

/// <summary>节点系统日志级别</summary>
public enum LogLevel
{
    Error = 0,
    Warning = 1,
    Info = 2,
    Verbose = 3
}

/// <summary>
/// 节点系统统一日志工具：按级别收敛控制台噪声
/// 平时保持 Warning（只看警告/错误），调试时用菜单 Tools/节点系统/日志级别 切 Info / Verbose
/// </summary>
public static class NodeLog
{
    /// <summary>当前日志级别（可在运行时调整）</summary>
    public static LogLevel Level = LogLevel.Warning;

    public static void Error(string message)
    {
        if (Level >= LogLevel.Error) Debug.LogError(message);
    }

    public static void Warning(string message)
    {
        if (Level >= LogLevel.Warning) Debug.LogWarning(message);
    }

    public static void Info(string message)
    {
        if (Level >= LogLevel.Info) Debug.Log(message);
    }

    public static void Verbose(string message)
    {
        if (Level >= LogLevel.Verbose) Debug.Log(message);
    }
}
