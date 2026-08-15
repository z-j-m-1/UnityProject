using UnityEngine;
using ZGameFramework.Core;

/// <summary>
/// 通讯-设置字符串变量事件
/// </summary>
public class ComSetVariableEvent<T> : ParameterizedEvent<ComSetVariableEvent<T>>
{
    public string targetName;
    public string variableName;
    public string guid;
    public T variableValue;

    /// <summary>解析完成回调（实际变量名, 实际GUID），供节点更新自身字段</summary>
    public System.Action<string, string> onResolved;

    public override void OnRecycled()
    {
        targetName = null;
        variableName = null;
        guid = null;
        variableValue = default;
        onResolved = null;
    }
}

/// <summary>
/// 通讯-获取字符串变量事件
/// </summary>
public class ComGetVariableEvent<T> : ParameterizedEvent<ComGetVariableEvent<T>>
{
    public string targetName;
    public string variableName;
    public string guid;
    public T defaultValue;

    /// <summary>回调（值, 实际变量名, 实际GUID）</summary>
    public System.Action<T, string, string> callback;

    public override void OnRecycled()
    {
        targetName = null;
        variableName = null;
        guid = null;
        defaultValue = default;
        callback = null;
    }
}