using UnityEngine;
using XNode;
using ZGameFramework.Core;

/// <summary>
/// 通讯-获取变量节点的泛型基类
/// </summary>
/// <typeparam name="T">变量类型</typeparam>
public abstract class ComGetVariableNode<T> : DataNode
{
    [Header("目标图名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string targetGraphName;

    [Header("变量名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string variableName;

    [Header("默认值")]
    public T defaultValue;

    [Output]
    public T outputValue;

    /// <summary>
    /// 子类实现请求具体的事件
    /// </summary>
    protected abstract void RequestVariable(string graphName, string varName, System.Action<T> callback);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(targetGraphName))
            return GetInputValue<string>("targetGraphName", targetGraphName);
        if (port.fieldName == nameof(variableName))
            return GetInputValue<string>("variableName", variableName);
        if (port.fieldName == nameof(outputValue))
        {
            // 触发获取变量事件（由子类实现）
            RequestVariable(
            GetInputValue<string>("targetGraphName", targetGraphName),
            GetInputValue<string>("variableName", variableName),
            (value) =>
            {
                outputValue = value;
                Debug.Log($"{GetType().Name}: 通讯获取到变量 '{GetInputValue<string>("targetGraphName", targetGraphName)}' = '{value}'");
            });


            return outputValue;

        }
            
            
        return null;
    }
}