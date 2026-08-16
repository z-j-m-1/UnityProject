using UnityEngine;
using XNode;

/// <summary>变换-旋转物体节点（对目标 Transform 增加旋转偏移）</summary>
[CreateNodeMenu("变换/旋转")]
[NodeTint("#44AAFF")]
[NodeWidth(300)]
public class RoteObjectNode : ComponentActionNode<Transform>
{
    [Header("旋转偏移量（欧拉角）")]
    public Vector3 roteOffset;

    protected override void Apply(Transform t)
    {
        t.rotation *= Quaternion.Euler(roteOffset);
    }
}
