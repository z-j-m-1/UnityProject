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
    public string targetName;

    [Header("变量名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string variableName;

    [Header("默认值")]
    public T defaultValue;

    [Output]
    public T outputValue;

    /// <summary>
    /// 触发通讯获取变量事件（泛型实现，请求目标图上的变量）
    /// </summary>
    protected void RequestVariable(string graphName, string varName, System.Action<T> callback)
    {
        ComGetVariableEvent<T>.Trigger(evt =>
        {
            evt.targetName = graphName;
            evt.variableName = varName;
            evt.defaultValue = defaultValue;
            evt.callback = callback;
        });
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(targetName))
            return GetInputValue<string>(nameof(targetName), targetName);
        if (port.fieldName == nameof(variableName))
            return GetInputValue<string>(nameof(variableName), variableName);
        if (port.fieldName == nameof(outputValue))
        {
            string graphName = GetInputValue<string>(nameof(targetName), targetName);
            string varName = GetInputValue<string>(nameof(variableName), variableName);

            RequestVariable(graphName, varName, value =>
            {
                outputValue = value;
                Debug.Log($"{GetType().Name}: 通讯获取到变量 '{graphName}.{varName}' = '{value}'");
            });

            return outputValue;
        }

        return null;
    }
}