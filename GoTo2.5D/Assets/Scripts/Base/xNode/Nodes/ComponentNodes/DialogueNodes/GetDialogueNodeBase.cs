using UnityEngine;
using XNode;

/// <summary>
/// 对话UI 取值节点基类（数据节点，不参与流程）：统一「目标物体输入端口 + 解析 DialogueBoxController」，
/// 子类各自重写 GetValue 读自己的输出字段（读之前调用 ResolveController()）。
/// 目标只认「目标物体」输入端口（由 取值/获取物体(自身|名称|引用) 提供）；
/// 输出口非序列化（运行时求值，规避场景引用写进图资产 / 跨场景重载失效）。
/// </summary>
public abstract class GetDialogueNodeBase : DataNode
{
    [Header("目标（接线：取值/获取物体(自身|名称|引用)）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject targetGameObject;

    /// <summary>解析目标对话框控制器（未接线 / 目标上没有该组件时警告并返回 null）</summary>
    protected DialogueBoxController ResolveController()
    {
        GameObject obj = GetInputValue<GameObject>(nameof(targetGameObject), null);
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把「取值/获取物体(自身|名称|引用)」接到目标端口）");
            return null;
        }

        DialogueBoxController box = obj.GetComponent<DialogueBoxController>();
        if (box == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 上没有 DialogueBoxController");
            return null;
        }

        return box;
    }
}
