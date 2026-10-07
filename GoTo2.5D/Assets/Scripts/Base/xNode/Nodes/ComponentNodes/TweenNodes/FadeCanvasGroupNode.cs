using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 插值-透明度：CanvasGroup.alpha 在 duration 秒内渐变到 targetAlpha（0=隐 1=显）。
/// 继承 <see cref="TweenNodeBase"/> → 自带**曲线插值**（默认线性 = 与加曲线前一致）。
/// 结束精确归位；曲线过冲时结果会被 Clamp 到 0~1（alpha 不允许越界）。
/// 目标物体由「目标物体」输入端口提供（接 取值/获取物体）。
/// </summary>
[CreateNodeMenu("插值/透明度")]
[NodeTint("#44AAFF")]
public class FadeCanvasGroupNode : TweenNodeBase
{
    [Header("目标透明度（0-1）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float targetAlpha = 0f;

    private CanvasGroup group;
    private float start;
    private float end;

    protected override bool ResolveTarget()
    {
        GameObject obj = ResolveTargetObject();
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把任一「取值/获取物体」节点接到目标端口）");
            return false;
        }

        group = obj.GetComponent<CanvasGroup>();
        if (group == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 没有 CanvasGroup");
            return false;
        }

        return true;
    }

    protected override bool CaptureStart()
    {
        start = group.alpha;
        end = Mathf.Clamp01(GetInputValue<float>(nameof(targetAlpha), targetAlpha));
        return true;
    }

    protected override void ApplyStep(float mix)
    {
        group.alpha = Mathf.Clamp01(Mix(start, end, mix));
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(targetAlpha))
            return GetInputValue<float>(nameof(targetAlpha), targetAlpha);
        return base.GetValue(port);
    }
}
