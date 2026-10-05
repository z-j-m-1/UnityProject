using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 插值-透明度：CanvasGroup.alpha 在 duration 秒内渐变到 targetAlpha（0=隐 1=显，逐帧插值，结束精确归位）。
/// 目标解析复用 ComponentActionNodeBase（GameObject 输入端口 &gt; Attached/ByName/Direct）。
/// </summary>
[CreateNodeMenu("插值/透明度")]
[NodeTint("#44AAFF")]
public class FadeCanvasGroupNode : ComponentActionNodeBase
{
    [Header("目标透明度（0-1）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float targetAlpha = 0f;

    [Header("持续时间（秒）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float duration = 1f;

    private CanvasGroup group;

    public override void Execute()
    {
        GameObject obj = ResolveTargetObject();
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未解析到目标物体（{target}）");
            return;
        }
        group = obj.GetComponent<CanvasGroup>();
        if (group == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 没有 CanvasGroup");
        }
    }

    public override IEnumerator GetFlow()
    {
        if (group == null) yield break;

        float start = group.alpha;
        float end = Mathf.Clamp01(GetInputValue<float>(nameof(targetAlpha), targetAlpha));
        float dur = Mathf.Max(0f, GetInputValue<float>(nameof(duration), duration));

        float t0 = Time.time;
        while (Time.time - t0 < dur)
        {
            float k = dur > 0f ? Mathf.Clamp01((Time.time - t0) / dur) : 1f;
            group.alpha = Mathf.Lerp(start, end, k);
            yield return null;
        }
        group.alpha = end;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(targetAlpha))
            return GetInputValue<float>(nameof(targetAlpha), targetAlpha);
        if (port.fieldName == nameof(duration))
            return GetInputValue<float>(nameof(duration), duration);
        return null;
    }
}