using UnityEngine;
using ZGameFramework.Core;

/// <summary>
/// 通讯-设置持久变量事件
/// </summary>
public class PersistentSetVariableEvent<T> : ParameterizedEvent<PersistentSetVariableEvent<T>>
{
    public PersistentVariableScope scope;
    public string variableName;
    public string guid;
    public T variableValue;

    /// <summary>解析完成回调（实际变量名, 实际GUID），供节点更新自身字段</summary>
    public System.Action<string, string> onResolved;

    public override void OnRecycled()
    {
        scope = default;
        variableName = null;
        guid = null;
        variableValue = default;
        onResolved = null;
    }
}

/// <summary>
/// 通讯-获取持久变量事件
/// </summary>
public class PersistentGetVariableEvent<T> : ParameterizedEvent<PersistentGetVariableEvent<T>>
{
    public PersistentVariableScope scope;
    public string variableName;
    public string guid;
    public T defaultValue;

    /// <summary>回调（值, 实际变量名, 实际GUID）</summary>
    public System.Action<T, string, string> callback;

    public override void OnRecycled()
    {
        scope = default;
        variableName = null;
        guid = null;
        defaultValue = default;
        callback = null;
    }
}

