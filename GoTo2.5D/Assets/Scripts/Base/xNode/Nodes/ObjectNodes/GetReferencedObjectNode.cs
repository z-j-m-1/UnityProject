using UnityEngine;
using XNode;

/// <summary>
/// 取值-获取物体（引用）：直接输出面板上拖入的物体引用（等价于组件节点的 Direct 目标模式）。
/// 该引用是序列化字段，会写进图资产——与原先组件节点「直接拖引用」的目标模式同一取舍：
/// 适合常驻物体，跨场景重载后可能失效的场景对象请改接「取值/获取物体(名称)」或用参数注入。
/// </summary>
[CreateNodeMenu("取值/获取物体(引用)")]
public class GetReferencedObjectNode : GetObjectNodeBase
{
    [Header("物体引用（直接拖）")]
    public GameObject reference;

    protected override GameObject Resolve()
    {
        return reference;
    }
}
