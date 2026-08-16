using UnityEngine;
using XNode;

/// <summary>动画-交叉淡入（Animator.CrossFade）</summary>
[CreateNodeMenu("动画/交叉淡入")]
public class CrossFadeAnimatorNode : ComponentActionNode<Animator>
{
    [Header("状态名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string stateName;

    [Header("过渡时长（秒）")]
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
        animator.CrossFade(name, GetInputValue<float>(nameof(transitionDuration), transitionDuration));
    }
}