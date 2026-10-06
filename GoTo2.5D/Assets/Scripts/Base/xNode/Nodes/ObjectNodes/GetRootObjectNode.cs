using UnityEngine;
using XNode;

/// <summary>
/// 取值-获取物体（根物体）：输出传入物体所在层级的**最顶层物体**（`Transform.root`）。
/// 相当于一路沿父物体向上走到顶（一次调用完成，无需串联多个「父物体」节点）。
/// 传入物体本身就是根物体时，返回它自己。
/// </summary>
[CreateNodeMenu("取值/获取物体(根物体)")]
public class GetRootObjectNode : GetObjectNodeBase
{
    [Header("子物体（取它所在的根物体）")]
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

        return obj.transform.root.gameObject;
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
