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
    public string targetGraphName;

    [Header("变量名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string variableName;

    [Header("变量值")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T variableValue;

    public override void Execute()
    {
        // 获取输入值
        string graphName = GetInputValue<string>("targetGraphName", this.targetGraphName);
        string varName = GetInputValue<string>("variableName", this.variableName);
        T varValue = GetInputValue<T>("variableValue", this.variableValue);

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

        // 触发具体的事件（由子类实现）
        TriggerEvent(graphName, varName, varValue);

        Debug.Log($"{GetType().Name}: 触发通讯设置变量事件 - 图:'{graphName}', 变量:'{varName}', 值:'{varValue}'");

        // 执行下一个节点
        base.Execute();
    }

    /// <summary>
    /// 子类实现触发具体的事件
    /// </summary>
    protected abstract void TriggerEvent(string graphName, string varName, T varValue);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(targetGraphName))
            return GetInputValue<string>("targetGraphName", targetGraphName);
        if (port.fieldName == nameof(variableName))
            return GetInputValue<string>("variableName", variableName);
        if (port.fieldName == nameof(variableValue))
            return GetInputValue<T>("variableValue", variableValue);
        return null;
    }
}