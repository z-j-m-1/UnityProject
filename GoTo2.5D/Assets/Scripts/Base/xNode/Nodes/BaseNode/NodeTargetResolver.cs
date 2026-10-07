using UnityEngine;

/// <summary>
/// 「目标物体端口」解析助手：统一读取节点上的 <c>targetGameObject</c> 输入端口并取组件 T。
///
/// 为什么需要它：C# 不支持多继承，而需要目标物体的节点分散在 FlowNode / DataNode /
/// ConditionWaitNode 等不同基类链上，同一个端口字段 + 同一段解析逻辑只能各自重写一遍。
/// 字段声明仍要各写一份（无法跨基类共享），但**解析逻辑**用本助手收成一处。
/// </summary>
public static class NodeTargetResolver
{
    /// <summary>
    /// 读 <paramref name="targetFieldName"/> 输入端口 → <c>GetComponent&lt;T&gt;()</c>；
    /// 任一步失败都会用节点的类型名打警告并返回 null。
    /// </summary>
    public static T Resolve<T>(BaseNode node, string targetFieldName = "targetGameObject") where T : Component
    {
        if (node == null) return null;

        GameObject obj = node.GetInputValue<GameObject>(targetFieldName, null);
        if (obj == null)
        {
            NodeLog.Warning($"{node.GetType().Name}: 未接入目标物体（请把任一「取值/获取物体」节点接到目标端口）");
            return null;
        }

        T component = obj.GetComponent<T>();
        if (component == null)
        {
            NodeLog.Warning($"{node.GetType().Name}: 目标 '{obj.name}' 上没有组件 {typeof(T).Name}");
        }
        return component;
    }
}
