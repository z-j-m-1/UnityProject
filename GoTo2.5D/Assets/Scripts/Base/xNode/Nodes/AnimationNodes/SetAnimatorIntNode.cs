using UnityEngine;
using XNode;

/// <summary>动画-整数参数（Animator.SetInteger）</summary>
[CreateNodeMenu("动画/整数参数")]
public class SetAnimatorIntNode : ComponentActionNode<Animator>
{
    [Header("参数名（Int）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string paramName;

    [Header("值")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int value;

    protected override void Apply(Animator animator)
    {
        string name = GetInputValue<string>(nameof(paramName), paramName);
        if (string.IsNullOrEmpty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 参数名为空");
            return;
        }
        animator.SetInteger(name, GetInputValue<int>(nameof(value), value));
    }
}