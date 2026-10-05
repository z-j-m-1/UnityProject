using Cinemachine;
using UnityEngine;
using XNode;

/// <summary>相机-虚拟相机优先级：调高优先级即切入该相机（CinemachineBrain 自动混合），设 0 切走。</summary>
[CreateNodeMenu("相机/虚拟相机优先级")]
[NodeTint("#FFAA44")]
public class SetVcamPriorityNode : ComponentActionNode<CinemachineVirtualCamera>
{
    [Header("优先级（高于其它虚拟相机即被激活）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int priority = 100;

    protected override void Apply(CinemachineVirtualCamera vcam)
    {
        vcam.Priority = GetInputValue<int>(nameof(priority), priority);
    }
}