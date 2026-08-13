using UnityEngine;
using ZGameFramework.Core;

/// <summary>
/// 通讯-获取自身TextMeshPro事件
/// </summary>
public class ComGetSelfTextMeshProEvent : ParameterizedEvent<ComGetSelfTextMeshProEvent>
{
    public string graphName;
    public string uiObjectName;           // UI对象名称
    public string defaultValue;
    public System.Action<string> callback;
    public override void OnRecycled()
    {
        graphName = null;
        uiObjectName = null;
        defaultValue = null;
        callback = null;
    }
}

/// <summary>
/// 通讯-设置自身TextMeshPro事件
/// </summary>
public class ComSetSelfTextMeshProEvent : ParameterizedEvent<ComSetSelfTextMeshProEvent>
{
    public string graphName;
    public string uiObjectName;           // UI对象名称
    public string textValue;
    public override void OnRecycled()
    {
        graphName = null;
        uiObjectName = null;
        textValue = null;
    }
}

/// <summary>
/// 通讯-获取自身Text事件
/// </summary>
public class ComGetSelfTextEvent : ParameterizedEvent<ComGetSelfTextEvent>
{
    public string graphName;
    public string uiObjectName;           // UI对象名称
    public string defaultValue;
    public System.Action<string> callback;
    public override void OnRecycled()
    {
        graphName = null;
        uiObjectName = null;
        defaultValue = null;
        callback = null;
    }
}

/// <summary>
/// 通讯-设置自身Text事件
/// </summary>
public class ComSetSelfTextEvent : ParameterizedEvent<ComSetSelfTextEvent>
{
    public string graphName;
    public string uiObjectName;           // UI对象名称
    public string textValue;
    public override void OnRecycled()
    {
        graphName = null;
        uiObjectName = null;
        textValue = null;
    }
}