using UnityEngine;
using XNode;
using ZGameFramework.Core;

/// <summary>
/// 通讯-设置变量节点的泛型基类
/// </summary>
/// <typeparam name="T">变量类型</typeparam>
public abstract class ComSetVariableNode<T> : FlowNode
{
    [Header("目标图名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string targetName;

    [Header("变量名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string variableName;

    [Header("变量值")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T variableValue;

    public override void Execute()
    {
        // 获取输入值
        string graphName = GetInputValue<string>(nameof(targetName), this.targetName);
        string varName = GetInputValue<string>(nameof(variableName), this.variableName);
        T varValue = GetInputValue<T>(nameof(variableValue), this.variableValue);

        // 验证参数
        if (string.IsNullOrEmpty(graphName))
        {
            Debug.LogError($"{GetType().Name}: 目标节点图名称不能为空");
            return;
        }

        if (string.IsNullOrEmpty(varName))
        {
            Debug.LogError($"{GetType().Name}: 变量名不能为空");
            return;
        }

        // 触发通讯设置变量事件（泛型实现）
        TriggerEvent(graphName, varName, varValue);

        Debug.Log($"{GetType().Name}: 触发通讯设置变量事件 - 图:'{graphName}', 变量:'{varName}', 值:'{varValue}'");

        // 执行下一个节点
        base.Execute();
    }

    /// <summary>
    /// 触发通讯设置变量事件（泛型实现，向目标图设置变量）
    /// </summary>
    protected void TriggerEvent(string graphName, string varName, T varValue)
    {
        ComSetVariableEvent<T>.Trigger(evt =>
        {
            evt.targetName = graphName;
            evt.variableName = varName;
            evt.variableValue = varValue;
        });
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(targetName))
            return GetInputValue<string>(nameof(targetName), targetName);
        if (port.fieldName == nameof(variableName))
            return GetInputValue<string>(nameof(variableName), variableName);
        if (port.fieldName == nameof(variableValue))
            return GetInputValue<T>(nameof(variableValue), variableValue);
        return null;
    }
}