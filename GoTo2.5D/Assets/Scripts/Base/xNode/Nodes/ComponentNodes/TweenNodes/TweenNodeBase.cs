using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 插值节点基类：统一「时长 + **曲线** + 逐帧插值 + 结束精确归位」。
/// **今后所有插值节点都继承本类**，就自动获得曲线插值能力。
///
/// 曲线作用在**混合系数**上：先算线性进度 k(0~1)，再取 <c>curve.Evaluate(k)</c> 作为最终系数。
/// 所以：
///   · 默认曲线是**线性**（<see cref="AnimationCurve.Linear"/>）→ 行为与加曲线之前**完全一致**，
///     旧节点升级后不会变（且 curve 为 null / 空时会自动退回线性，防御序列化意外）；
///   · 曲线值 &gt;1 或 &lt;0 → **过冲 / 回弹**（超出目标值再回来），这是有意支持的，不做裁剪；
///     需要限制范围的子类自己在 ApplyStep 里 Clamp（如透明度）。
///
/// 流程语义：<c>GetFlow()</c> 会跑完整个插值才继续下一个节点 —— **这本身就是"等待结束"**，
/// 所以不需要额外的"是否等待"开关或"是否完成"输出端口（要并行就接「流程/并行」）。
/// </summary>
public abstract class TweenNodeBase : ComponentActionNodeBase
{
    [Header("持续时间（秒）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float duration = 1f;

    [Header("插值曲线（横轴 0~1 进度，纵轴 0~1 混合系数；默认线性）")]
    public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    /// <summary>目标是否解析成功（Execute 时设置）</summary>
    protected bool TargetReady { get; private set; }

    public override void Execute()
    {
        TargetReady = false;
        if (!ResolveTarget()) return;
        TargetReady = true;
    }

    public override IEnumerator GetFlow()
    {
        if (!TargetReady) yield break;
        if (!CaptureStart()) yield break;

        float dur = Mathf.Max(0f, GetInputValue<float>(nameof(duration), duration));

        if (dur > 0f)
        {
            float t0 = Time.time;
            while (Time.time - t0 < dur)
            {
                float k = Mathf.Clamp01((Time.time - t0) / dur);
                ApplyStep(Evaluate(k));
                yield return null;
            }
        }

        // 结束：用曲线末端值（Mix 会在系数 >= 1 时精确取终点，避免浮点残差）
        ApplyStep(Evaluate(1f));
    }

    /// <summary>曲线求值：线性进度 k(0~1) → 混合系数</summary>
    protected float Evaluate(float k)
    {
        if (curve == null || curve.length == 0) return k;
        return curve.Evaluate(k);
    }

    /// <summary>按混合系数取插值结果；系数 >= 1 时直接返回终点（保证"结束精确归位"）</summary>
    protected static float Mix(float start, float end, float mix)
    {
        return mix >= 1f ? end : Mathf.Lerp(start, end, mix);
    }

    /// <summary>按混合系数取插值结果；系数 >= 1 时直接返回终点（保证"结束精确归位"）</summary>
    protected static Vector3 Mix(Vector3 start, Vector3 end, float mix)
    {
        return mix >= 1f ? end : Vector3.Lerp(start, end, mix);
    }

    /// <summary>解析目标组件；失败请自行警告并返回 false</summary>
    protected abstract bool ResolveTarget();

    /// <summary>记录起始值（每次插值开始时调用一次）；返回 false 表示配置无效、放弃本次插值（请自行警告）</summary>
    protected abstract bool CaptureStart();

    /// <summary>按混合系数写入一帧（系数已过曲线，可能超出 0~1 = 过冲）</summary>
    protected abstract void ApplyStep(float mix);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(duration))
            return GetInputValue<float>(nameof(duration), duration);
        return null;
    }
}
