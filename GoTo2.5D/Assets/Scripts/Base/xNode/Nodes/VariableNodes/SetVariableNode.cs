using UnityEngine;
using XNode;

/// <summary>
/// 统一设置变量节点基类 - 通过 source 选择操作对象（本图/跨图/房间/全局）
/// </summary>
/// <typeparam name="T">变量类型</typeparam>
public abstract class SetVariableNode<T> : FlowNode
{
    [Header("操作对象")]
    public VariableSource source = VariableSource.Self;

    [Header("目标名（跨图时使用）")]
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
        string varName = GetInputValue<string>(nameof(variableName), variableName);
        T varValue = GetInputValue<T>(nameof(variableValue), variableValue);

        if (string.IsNullOrEmpty(varName))
        {
            Debug.LogError($"{GetType().Name}: 变量名不能为空");
            return;
        }

        switch (source)
        {
            case VariableSource.Self:
                if (graph is BaseNodeGraph selfGraph)
                {
                    selfGraph.Set(varName, varValue);
                }
                break;

            case VariableSource.ExternalGraph:
                ComSetVariableEvent<T>.Trigger(evt =>
                {
                    evt.targetName = GetInputValue<string>(nameof(targetName), targetName);
                    evt.variableName = varName;
                    evt.variableValue = varValue;
                });
                break;

            case VariableSource.Room:
            case VariableSource.Global:
                PersistentVariableScope scope = source == VariableSource.Room ? PersistentVariableScope.Room : PersistentVariableScope.Global;
                PersistentVariableManager.GetManager(scope);
                PersistentSetVariableEvent<T>.Trigger(evt =>
                {
                    evt.scope = scope;
                    evt.variableName = varName;
                    evt.variableValue = varValue;
                });
                break;
        }

        // 执行下一个节点
        base.Execute();
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
