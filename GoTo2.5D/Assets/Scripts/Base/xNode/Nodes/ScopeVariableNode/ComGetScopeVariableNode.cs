using UnityEngine;
using XNode;

/// <summary>
/// 通讯-按作用域获取持久变量节点的泛型基类
/// </summary>
/// <typeparam name="T">变量类型</typeparam>
public abstract class ComGetScopeVariableNode<T> : DataNode
{
    [Header("变量作用域")]
    public PersistentVariableScope scope;

    [Header("变量名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string variableName;

    [Header("默认值")]
    public T defaultValue;

    [Output]
    public T outputValue;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(variableName))
            return GetInputValue<string>(nameof(variableName), variableName);
        if (port.fieldName == nameof(outputValue))
        {
            string varName = GetInputValue<string>(nameof(variableName), variableName);

            // 确保管理器已创建并完成事件订阅
            PersistentVariableManager.GetManager(scope);

            PersistentGetVariableEvent<T>.Trigger(evt =>
            {
                evt.scope = scope;
                evt.variableName = varName;
                evt.defaultValue = defaultValue;
                evt.callback = value =>
                {
                    outputValue = value;
                    Debug.Log($"{GetType().Name}: 通讯获取到持久变量 '{scope}.{varName}' = '{value}'");
                };
            });

            return outputValue;
        }

        return null;
    }
}
