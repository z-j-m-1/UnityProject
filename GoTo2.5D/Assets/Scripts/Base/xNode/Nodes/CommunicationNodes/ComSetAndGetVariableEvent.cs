using UnityEngine;
using ZGameFramework.Core;

/// <summary>
/// 通讯-设置字符串变量事件
/// </summary>
public class ComSetVariableEvent<T> : ParameterizedEvent<ComSetVariableEvent<T>>
{
    public string targetName;
    public string variableName;
    public T variableValue;

    public override void OnRecycled()
    {
        targetName = null;
        variableName = null;
        variableValue = default;
    }
}

/// <summary>
/// 通讯-获取字符串变量事件
/// </summary>
public class ComGetVariableEvent<T> : ParameterizedEvent<ComGetVariableEvent<T>>
{
    public string targetName;
    public string variableName;
    public T defaultValue;
    public System.Action<T> callback;

    public override void OnRecycled()
    {
        targetName = null;
        variableName = null;
        defaultValue = default;
        callback = null;
    }
}