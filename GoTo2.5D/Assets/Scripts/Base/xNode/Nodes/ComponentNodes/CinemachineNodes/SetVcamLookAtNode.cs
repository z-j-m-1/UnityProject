using Cinemachine;
using UnityEngine;
using XNode;

/// <summary>相机-设置注视目标：vcam.LookAt = 目标（接「获取物体」/「参数/输入/物体」）。</summary>
[CreateNodeMenu("相机/设置注视")]
[NodeTint("#FFAA44")]
public class SetVcamLookAtNode : ComponentActionNode<CinemachineVirtualCamera>
{
    [Header("注视目标（可接 获取物体 / 参数输入/物体）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject lookAtTarget;

    protected override void Apply(CinemachineVirtualCamera vcam)
    {
        GameObject target = GetInputValue<GameObject>(nameof(lookAtTarget), null);
        vcam.LookAt = target != null ? target.transform : null;
        if (target == null)
        {
            NodeLog.Warning($"{GetType().Name}: 注视目标为空");
        }
    }
}