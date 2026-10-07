using UnityEngine;
using XNode;

/// <summary>
/// 组件动作节点的非泛型基类（供自定义编辑器按类型定位）：
/// 只提供统一的「目标物体输入端口」——目标由节点图上的获取物体节点显式指定
///（取值/获取物体：自身 / 名称 / 全场景 / 父物体 / 子物体 / 根物体）。
/// 子类只关心拿到目标后做什么。
/// </summary>
public abstract class ComponentActionNodeBase : FlowNode
{
    [Header("目标物体（接线：取值/获取物体）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject targetGameObject;

    /// <summary>解析目标物体：只取已连线的 GameObject 输入端口（未接线返回 null）</summary>
    protected GameObject ResolveTargetObject()
    {
        return GetInputValue<GameObject>(nameof(targetGameObject), null);
    }
}

/// <summary>
/// 组件动作节点基类 - 统一"目标解析 + 组件获取"，子类只实现 Apply 做具体动作
/// 例：移动/旋转/缩放（Transform）、播放（AudioSource）、状态（Animator）、虚拟相机、震源等
/// </summary>
/// <typeparam name="T">目标组件类型</typeparam>
public abstract class ComponentActionNode<T> : ComponentActionNodeBase where T : Component
{
    public override void Execute()
    {
        GameObject obj = ResolveTargetObject();
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把「取值/获取物体」接到目标端口）");
            return;
        }

        T component = obj.GetComponent<T>();
        if (component == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 上没有组件 {typeof(T).Name}");
            return;
        }

        Apply(component);
        NodeLog.Info($"{GetType().Name}: 已对 '{obj.name}' 执行 {typeof(T).Name} 动作");
    }

    /// <summary>子类实现具体动作</summary>
    protected abstract void Apply(T component);
}
