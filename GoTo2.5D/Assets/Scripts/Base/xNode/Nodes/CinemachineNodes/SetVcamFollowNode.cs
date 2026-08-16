using Cinemachine;
using UnityEngine;
using XNode;

/// <summary>相机-设置跟随目标：vcam.Follow = 目标（接「获取物体」/「参数/输入/物体」）。</summary>
[CreateNodeMenu("相机/设置跟随")]
[NodeTint("#FFAA44")]
public class SetVcamFollowNode : ComponentActionNode<CinemachineVirtualCamera>
{
    [Header("跟随目标（可接 获取物体 / 参数输入/物体）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject followTarget;

    protected override void Apply(CinemachineVirtualCamera vcam)
    {
        GameObject target = GetInputValue<GameObject>(nameof(followTarget), null);
        vcam.Follow = target != null ? target.transform : null;
        if (target == null)
        {
            NodeLog.Warning($"{GetType().Name}: 跟随目标为空");
        }
    }
}