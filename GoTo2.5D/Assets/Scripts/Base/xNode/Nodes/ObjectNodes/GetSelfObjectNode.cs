using UnityEngine;
using XNode;

/// <summary>
/// 取值-获取物体（自身）：直接输出「图绑定物体」，即当前执行器所在物体（等价于组件节点的 Attached 目标模式）。
/// 可接线到任意 ComponentActionNode 的 targetGameObject 输入端口。
/// </summary>
[CreateNodeMenu("取值/获取物体(自身)")]
public class GetSelfObjectNode : GetObjectNodeBase
{
    protected override GameObject Resolve()
    {
        return AttachedObject;
    }
}
