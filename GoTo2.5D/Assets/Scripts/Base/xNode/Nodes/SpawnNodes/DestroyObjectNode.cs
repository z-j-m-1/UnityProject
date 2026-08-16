using UnityEngine;
using XNode;

/// <summary>
/// 生成-销毁物体：目标 = GameObject 输入端口（优先）> 直接引用字段。
/// 可延迟销毁（0 = 立即）。
/// </summary>
[CreateNodeMenu("生成/销毁物体")]
[NodeTint("#88CC44")]
public class DestroyObjectNode : FlowNode
{
    [Header("目标物体（优先接输入端口，未接线用下方直接引用）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject target;

    [Header("目标引用（输入端口未接线时使用）")]
    public GameObject targetObject;

    [Header("延迟秒数（0 = 立即销毁）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float delay;

    public override void Execute()
    {
        GameObject obj = GetInputValue<GameObject>(nameof(target), null);
        if (obj == null)
        {
            obj = targetObject;
        }
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未指定目标物体");
            return;
        }

        float d = GetInputValue<float>(nameof(delay), delay);
        GameObject.Destroy(obj, d);
        NodeLog.Info($"{GetType().Name}: 已销毁 '{obj.name}'（延迟 {d}s）");
        base.Execute();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(target))
            return GetInputValue<GameObject>(nameof(target), null);
        if (port.fieldName == nameof(delay))
            return GetInputValue<float>(nameof(delay), delay);
        return null;
    }
}