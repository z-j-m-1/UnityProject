using UnityEngine;
using XNode;

/// <summary>组件目标解析方式</summary>
public enum ComponentTarget
{
    /// <summary>节点图绑定的物体（默认）</summary>
    Attached,
    /// <summary>图绑定物体的子物体（按名称查找）</summary>
    ByName,
    /// <summary>直接拖引用</summary>
    Direct
}

/// <summary>组件动作节点的非泛型基类（供自定义编辑器按类型定位）</summary>
public abstract class ComponentActionNodeBase : FlowNode
{
}

/// <summary>
/// 组件动作节点基类 - 统一"目标解析 + 组件获取"，子类只实现 Apply 做具体动作
/// 例：移动/旋转/缩放（Transform）、播放（AudioSource）、状态（Animator）等
/// </summary>
/// <typeparam name="T">目标组件类型</typeparam>
public abstract class ComponentActionNode<T> : ComponentActionNodeBase where T : Component
{
    [Header("目标")]
    public ComponentTarget target = ComponentTarget.Attached;

    [Header("目标名称（ByName 时用）")]
    public string targetName;

    [Header("目标引用（Direct 时用）")]
    public GameObject targetObject;

    [Header("目标（输入端口，未接线时用上方目标模式）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject targetGameObject;

    public override void Execute()
    {
        // 输入端口优先：已连线的 GameObject（来自「取值/获取物体」节点）> 目标模式解析
        GameObject obj = GetInputValue<GameObject>(nameof(targetGameObject), null);
        if (obj == null)
        {
            obj = ResolveTarget();
        }
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未解析到目标物体（{target}）");
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

    private GameObject ResolveTarget()
    {
        BaseNodeGraph nodeGraph = graph as BaseNodeGraph;
        switch (target)
        {
            case ComponentTarget.Attached:
                return nodeGraph != null ? nodeGraph.GetAttachedObject() : null;

            case ComponentTarget.ByName:
                GameObject attached = nodeGraph != null ? nodeGraph.GetAttachedObject() : null;
                if (attached == null || string.IsNullOrEmpty(targetName)) return null;
                Transform child = attached.transform.Find(targetName);
                return child != null ? child.gameObject : null;

            case ComponentTarget.Direct:
                return targetObject;
        }
        return null;
    }
}
