using UnityEngine;
using XNode;
using ZGameFramework.Core;

/// <summary>
/// 通讯-获取变量节点的泛型基类
/// </summary>
/// <typeparam name="T">变量类型</typeparam>
public abstract class ComGetVariableNode<T> : FlowNode
{
    [Header("目标图名称")]
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public string targetGraphName;

    [Header("变量名")]
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    public string variableName;

    [Header("默认值")]
    public T defaultValue;

    [Output]
    public T outputValue;

    public override void Execute()
    {
        string graphName = GetInputValue<string>("targetGraphName", this.targetGraphName);
        string varName = GetInputValue<string>("variableName", this.variableName);

        if (string.IsNullOrEmpty(graphName) || string.IsNullOrEmpty(varName))
        {
            Debug.LogError($"{GetType().Name}: 参数不能为空");
            outputValue = defaultValue;
            base.Execute();
            return;
        }

        // 触发获取变量事件（由子类实现）
        RequestVariable(graphName, varName, (value) =>
        {
            outputValue = value;
            Debug.Log($"{GetType().Name}: 通讯获取到变量 '{varName}' = '{value}'");
            base.Execute();
        });
    }

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
            return outputValue;
        return null;
    }
}