using UnityEngine;
using ZGameFramework.Core;

/// <summary>
/// 通讯-设置持久变量事件
/// </summary>
public class PersistentSetVariableEvent<T> : ParameterizedEvent<PersistentSetVariableEvent<T>>
{
    public PersistentVariableScope scope;
    public string variableName;
    public T variableValue;

    public override void OnRecycled()
    {
        scope = default;
        variableName = null;
        variableValue = default;
    }
}

/// <summary>
/// 通讯-获取持久变量事件
/// </summary>
public class PersistentGetVariableEvent<T> : ParameterizedEvent<PersistentGetVariableEvent<T>>
{
    public PersistentVariableScope scope;
    public string variableName;
    public T defaultValue;
    public System.Action<T> callback;

    public override void OnRecycled()
    {
        scope = default;
        variableName = null;
        defaultValue = default;
        callback = null;
    }
}
