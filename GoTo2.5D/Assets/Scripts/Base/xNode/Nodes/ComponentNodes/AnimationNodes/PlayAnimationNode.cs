using UnityEngine;
using XNode;

/// <summary>动画播放模式</summary>
public enum AnimationPlayMode
{
    /// <summary>Animator.Play：立即切换（无过渡）</summary>
    Play,
    /// <summary>Animator.CrossFade：带过渡时长交叉淡入</summary>
    CrossFade
}

/// <summary>
/// 动画-播放（Animator）：**一个节点覆盖 Play 与 CrossFade**（模式枚举），
/// 原「动画/交叉淡入」（CrossFadeAnimatorNode）已合并删除。
/// 状态名可接线；过渡时长仅 CrossFade 模式使用。
/// </summary>
[CreateNodeMenu("动画/播放")]
public class PlayAnimationNode : ComponentActionNode<Animator>
{
    [Header("状态名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string stateName;

    [Header("模式")]
    public AnimationPlayMode mode = AnimationPlayMode.Play;

    [Header("过渡时长（秒，仅 CrossFade 用）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float transitionDuration = 0.25f;

    protected override void Apply(Animator animator)
    {
        string name = GetInputValue<string>(nameof(stateName), stateName);
        if (string.IsNullOrEmpty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 状态名为空");
            return;
        }

        if (mode == AnimationPlayMode.CrossFade)
        {
            float duration = GetInputValue<float>(nameof(transitionDuration), transitionDuration);
            animator.CrossFade(name, duration);
            NodeLog.Verbose($"{GetType().Name}: CrossFade('{name}', {duration}s)");
        }
        else
        {
            animator.Play(name);
            NodeLog.Verbose($"{GetType().Name}: Play('{name}')");
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(stateName))
            return GetInputValue<string>(nameof(stateName), stateName);
        if (port.fieldName == nameof(transitionDuration))
            return GetInputValue<float>(nameof(transitionDuration), transitionDuration);
        return null;
    }
}
