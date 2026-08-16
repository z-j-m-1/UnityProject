using UnityEngine;
using XNode;

/// <summary>变换-缩放物体（目标 Transform 增加缩放偏移）</summary>
[CreateNodeMenu("变换/缩放")]
[NodeTint("#44AAFF")]
public class ScaleObjectNode : ComponentActionNode<Transform>
{
    [Header("缩放偏移")]
    public Vector3 scaleOffset = Vector3.one;

    protected override void Apply(Transform t)
    {
        t.localScale += scaleOffset;
    }
}
