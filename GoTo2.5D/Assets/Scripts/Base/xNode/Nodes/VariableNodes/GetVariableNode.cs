using UnityEngine;
using XNode;

/// <summary>
/// 统一获取变量节点基类 - 通过 source 选择操作对象（本图/跨图/房间/全局）
/// 名字优先 + GUID 兜底解析，自动记录/修正变量 GUID
/// </summary>
/// <typeparam name="T">变量类型</typeparam>
public abstract class GetVariableNode<T> : DataNode
{
    [Header("操作对象")]
    public VariableSource source = VariableSource.Self;

    [Header("目标图名（跨图时使用）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string targetName;

    [Header("变量名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string variableName;

    [Header("变量 GUID（自动追踪，调试用）")]
    [Tooltip("自动记录/修正的变量 GUID，用于变量改名后兜底解析")]
    public string variableGuid;

    [Header("默认值")]
    public T defaultValue;

    [Output]
    public T outputValue;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(targetName))
            return GetInputValue<string>(nameof(targetName), targetName);
        if (port.fieldName == nameof(variableName))
            return GetInputValue<string>(nameof(variableName), variableName);
        if (port.fieldName == nameof(outputValue))
        {
            string varName = GetInputValue<string>(nameof(variableName), variableName);

            // 先重置输出，避免显示上一次运行的残留值
            outputValue = defaultValue;

            switch (source)
            {
                case VariableSource.Self:
                    if (graph is BaseNodeGraph selfGraph)
                    {
                        if (selfGraph.TryGetVariable(varName, variableGuid, out T value, out string actualName, out string actualGuid))
                        {
                            outputValue = value;
                            ApplyResolved(actualName, actualGuid);
                        }
                        else
                        {
                            outputValue = selfGraph.Get(varName, defaultValue);
                        }
                    }
                    break;

                case VariableSource.ExternalGraph:
                    RequestExternalGraph(GetInputValue<string>(nameof(targetName), targetName), varName);
                    break;

                case VariableSource.Room:
                case VariableSource.Global:
                    RequestPersistent(varName);
                    break;
            }

            return outputValue;
        }

        return null;
    }

    /// <summary>跨图通讯请求（同步事件，回调立即设置 outputValue）</summary>
    private void RequestExternalGraph(string graphName, string varName)
    {
        ComGetVariableEvent<T>.Trigger(evt =>
        {
            evt.targetName = graphName;
            evt.variableName = varName;
            evt.guid = variableGuid;
            evt.defaultValue = defaultValue;
            evt.callback = (value, actualName, actualGuid) =>
            {
                outputValue = value;
                ApplyResolved(actualName, actualGuid);
            };
        });
    }

    /// <summary>持久变量请求（房间/全局）</summary>
    private void RequestPersistent(string varName)
    {
        PersistentVariableScope scope = source == VariableSource.Room ? PersistentVariableScope.Room : PersistentVariableScope.Global;

        // 确保管理器已创建并完成事件订阅
        PersistentVariableManager.GetManager(scope);

        PersistentGetVariableEvent<T>.Trigger(evt =>
        {
            evt.scope = scope;
            evt.variableName = varName;
            evt.guid = variableGuid;
            evt.defaultValue = defaultValue;
            evt.callback = (value, actualName, actualGuid) =>
            {
                outputValue = value;
                ApplyResolved(actualName, actualGuid);
            };
        });
    }

    /// <summary>用解析结果修正节点上的变量名/GUID（双向适配）</summary>
    private void ApplyResolved(string actualName, string actualGuid)
    {
        if (!string.IsNullOrEmpty(actualName) && actualName != variableName)
        {
            variableName = actualName;
            Debug.Log($"{GetType().Name}: 变量名已更新为 '{actualName}'");
        }
        if (!string.IsNullOrEmpty(actualGuid) && actualGuid != variableGuid)
        {
            variableGuid = actualGuid;
        }
    }
}

