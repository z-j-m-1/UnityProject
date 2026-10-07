using UnityEngine;
using XNode;

/// <summary>
/// 生成-销毁物体：目标 = GameObject 输入端口（接 取值/获取物体 系列节点）。
/// 可延迟销毁（0 = 立即）。
/// </summary>
[CreateNodeMenu("生成/销毁物体")]
[NodeTint("#88CC44")]
public class DestroyObjectNode : FlowNode
{
    [Header("目标物体（接线：任一「取值/获取物体」节点）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject target;

    [Header("延迟秒数（0 = 立即销毁）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float delay;

    public override void Execute()
    {
        GameObject obj = GetInputValue<GameObject>(nameof(target), null);
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把任一「取值/获取物体」节点接到目标端口）");
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
