using UnityEngine;
using XNode;

/// <summary>
/// 取值-获取物体（父物体）：输出「子物体」的父物体（Transform.parent）。
/// 需要先用「取值/获取物体(自身|名称|引用|全场景)」拿到子物体，再接到本节点的「子物体」端口。
/// 子物体为空、或它本身是根物体（没有父物体）时给出警告并返回 null。
/// 输出口非序列化（运行时求值，规避场景引用写进图资产 / 跨场景重载失效）。
/// </summary>
[CreateNodeMenu("取值/获取物体(父物体)")]
public class GetParentObjectNode : GetObjectNodeBase
{
    [Header("子物体（取它的父物体）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject child;

    protected override GameObject Resolve()
    {
        GameObject obj = GetInputValue<GameObject>(nameof(child), null);
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入子物体（请把「取值/获取物体」接到「子物体」端口）");
            return null;
        }

        Transform parent = obj.transform.parent;
        if (parent == null)
        {
            NodeLog.Warning($"{GetType().Name}: '{obj.name}' 是根物体，没有父物体");
            return null;
        }

        return parent.gameObject;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(child))
        {
            return GetInputValue<GameObject>(nameof(child), null);
        }
        return base.GetValue(port);
    }
}
