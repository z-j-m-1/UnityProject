using Cinemachine;
using UnityEngine;
using XNode;

/// <summary>虚拟相机的目标类别</summary>
public enum VcamTargetMode
{
    /// <summary>跟随（vcam.Follow）</summary>
    Follow,
    /// <summary>注视（vcam.LookAt）</summary>
    LookAt
}

/// <summary>
/// 相机-跟随·注视：**一个节点覆盖原来的两个**
/// （「相机/设置跟随」SetVcamFollowNode / 「相机/设置注视」SetVcamLookAtNode，
/// 两者除写入的属性外逐行同构）。
/// 目标留空会写入 null 并警告 —— 与原行为一致。
/// </summary>
[CreateNodeMenu("相机/跟随·注视")]
[NodeTint("#FFAA44")]
public class SetVcamTargetNode : ComponentActionNode<CinemachineVirtualCamera>
{
    [Header("目标类别")]
    public VcamTargetMode mode = VcamTargetMode.Follow;

    [Header("目标（可接 获取物体 / 参数输入/物体）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject target;

    protected override void Apply(CinemachineVirtualCamera vcam)
    {
        GameObject go = GetInputValue<GameObject>(nameof(target), null);
        Transform t = go != null ? go.transform : null;

        if (mode == VcamTargetMode.LookAt)
        {
            vcam.LookAt = t;
            if (go == null) NodeLog.Warning($"{GetType().Name}: 注视目标为空");
        }
        else
        {
            vcam.Follow = t;
            if (go == null) NodeLog.Warning($"{GetType().Name}: 跟随目标为空");
        }
    }
}
