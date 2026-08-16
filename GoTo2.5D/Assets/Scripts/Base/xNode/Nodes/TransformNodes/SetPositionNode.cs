using UnityEngine;
using XNode;

/// <summary>变换-设置物体位置</summary>
[CreateNodeMenu("变换/设置位置")]
[NodeTint("#44AAFF")]
public class SetPositionNode : ComponentActionNode<Transform>
{
    [Header("目标位置")]
    public Vector3 targetPosition;

    protected override void Apply(Transform t)
    {
        t.position = targetPosition;
    }
}
