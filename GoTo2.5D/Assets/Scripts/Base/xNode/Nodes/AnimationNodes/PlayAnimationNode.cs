using UnityEngine;
using XNode;

/// <summary>动画-播放（Animator）</summary>
[CreateNodeMenu("动画/播放")]
public class PlayAnimationNode : ComponentActionNode<Animator>
{
    [Header("状态名")]
    public string stateName;

    protected override void Apply(Animator animator)
    {
        if (string.IsNullOrEmpty(stateName)) return;
        animator.Play(stateName);
    }
}
