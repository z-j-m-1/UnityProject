using UnityEngine;
using XNode;

/// <summary>
/// 统一设置变量节点的非泛型基类（供自定义编辑器按类型定位）
/// </summary>
public abstract class SetVariableNodeBase : FlowNode
{
}

/// <summary>
/// 统一设置变量节点基类 - 通过 source 选择操作对象（本图/跨图/房间/全局）
/// 名字优先 + GUID 兜底解析，自动记录/修正变量 GUID
/// </summary>
/// <typeparam name="T">变量类型</typeparam>
public abstract class SetVariableNode<T> : SetVariableNodeBase
{
    [Header("操作对象")]
    public VariableSource source = VariableSource.Self;

    [Header("目标名（跨图时使用）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string targetName;

    [Header("变量名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string variableName;

    [Header("变量 GUID（自动追踪，调试用）")]
    [Tooltip("自动记录/修正的变量 GUID，用于变量改名后兜底解析")]
    public string variableGuid;

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
                    if (selfGraph.TrySetVariable(varName, variableGuid, varValue, out string actualName, out string actualGuid))
                    {
                        ApplyResolved(actualName, actualGuid);
                    }
                    else
                    {
                        // 名字和GUID都找不到：按名字直接创建/设置
                        selfGraph.Set(varName, varValue);
                    }
                }
                break;

            case VariableSource.ExternalGraph:
                ComSetVariableEvent<T>.Trigger(evt =>
                {
                    evt.targetName = GetInputValue<string>(nameof(targetName), targetName);
                    evt.variableName = varName;
                    evt.guid = variableGuid;
                    evt.variableValue = varValue;
                    evt.onResolved = (actualName, actualGuid) => ApplyResolved(actualName, actualGuid);
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
                    evt.guid = variableGuid;
                    evt.variableValue = varValue;
                    evt.onResolved = (actualName, actualGuid) => ApplyResolved(actualName, actualGuid);
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

    /// <summary>用解析结果修正节点上的变量名/GUID（双向适配）</summary>
    private void ApplyResolved(string actualName, string actualGuid)
    {
        if (!string.IsNullOrEmpty(actualName) && actualName != variableName)
        {
            variableName = actualName;
            NodeLog.Info($"{GetType().Name}: 变量名已更新为 '{actualName}'");
        }
        if (!string.IsNullOrEmpty(actualGuid) && actualGuid != variableGuid)
        {
            variableGuid = actualGuid;
        }
    }
}

