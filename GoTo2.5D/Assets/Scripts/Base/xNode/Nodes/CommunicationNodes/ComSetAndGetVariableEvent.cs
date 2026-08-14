using UnityEngine;
using ZGameFramework.Core;

/// <summary>
/// 通讯-设置字符串变量事件
/// </summary>
public class ComSetStringVariableEvent : ParameterizedEvent<ComSetStringVariableEvent>
{
    public string targetGraphName;
    public string variableName;
    public string variableValue;

    public override void OnRecycled()
    {
        targetGraphName = null;
        variableName = null;
        variableValue = null;
    }
}

/// <summary>
/// 通讯-设置布尔变量事件
/// </summary>
public class ComSetBoolVariableEvent : ParameterizedEvent<ComSetBoolVariableEvent>
{
    public string targetGraphName;
    public string variableName;
    public bool variableValue;

    public override void OnRecycled()
    {
        targetGraphName = null;
        variableName = null;
        variableValue = false;
    }
}

/// <summary>
/// 通讯-设置整数变量事件
/// </summary>
public class ComSetIntVariableEvent : ParameterizedEvent<ComSetIntVariableEvent>
{
    public string targetGraphName;
    public string variableName;
    public int variableValue;

    public override void OnRecycled()
    {
        targetGraphName = null;
        variableName = null;
        variableValue = 0;
    }
}

/// <summary>
/// 通讯-获取字符串变量事件
/// </summary>
public class ComGetVariableEvent<T> : ParameterizedEvent<ComGetVariableEvent<T>>
{
    public string targetGraphName;
    public string variableName;
    public T defaultValue;
    public System.Action<T> callback;

    public override void OnRecycled()
    {
        targetGraphName = null;
        variableName = null;
        defaultValue = default;
        callback = null;
    }
}