using UnityEngine;
using XNode;

/// <summary>
/// 通讯-按作用域设置持久变量节点的泛型基类
/// </summary>
/// <typeparam name="T">变量类型</typeparam>
public abstract class ComSetScopeVariableNode<T> : FlowNode
{
    [Header("变量作用域")]
    public PersistentVariableScope scope;

    [Header("变量名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string variableName;

    [Header("变量值")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public T variableValue;

    public override void Execute()
    {
        string varName = GetInputValue<string>(nameof(variableName), variableName);
        T varValue = GetInputValue<T>(nameof(variableValue), variableValue);

        if (string.IsNullOrEmpty(varName))
        {
            Debug.LogError($"{GetType().Name}: 变量名不能为空");
            return;
        }

        // 确保管理器已创建并完成事件订阅
        PersistentVariableManager.GetManager(scope);

        PersistentSetVariableEvent<T>.Trigger(evt =>
        {
            evt.scope = scope;
            evt.variableName = varName;
            evt.variableValue = varValue;
        });

        Debug.Log($"{GetType().Name}: 触发通讯设置持久变量事件 - 作用域:'{scope}', 变量:'{varName}', 值:'{varValue}'");

        // 执行下一个节点
        base.Execute();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(variableName))
            return GetInputValue<string>(nameof(variableName), variableName);
        if (port.fieldName == nameof(variableValue))
            return GetInputValue<T>(nameof(variableValue), variableValue);
        return null;
    }
}
