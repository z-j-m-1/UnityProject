using UnityEngine;
using XNode;

/// <summary>变换-设置物体旋转（欧拉角）</summary>
[CreateNodeMenu("变换/设置旋转")]
[NodeTint("#44AAFF")]
public class SetRotationNode : ComponentActionNode<Transform>
{
    [Header("目标旋转（欧拉角）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 rotation;

    protected override void Apply(Transform t)
    {
        t.rotation = Quaternion.Euler(GetInputValue<Vector3>(nameof(rotation), rotation));
    }
}
