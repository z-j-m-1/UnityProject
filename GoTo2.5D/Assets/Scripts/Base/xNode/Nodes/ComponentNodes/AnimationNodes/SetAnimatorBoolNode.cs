using UnityEngine;
using XNode;

/// <summary>动画-布尔参数（Animator.SetBool）</summary>
[CreateNodeMenu("动画/布尔参数")]
public class SetAnimatorBoolNode : ComponentActionNode<Animator>
{
    [Header("参数名（Bool）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string paramName;

    [Header("值")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public bool value;

    protected override void Apply(Animator animator)
    {
        string name = GetInputValue<string>(nameof(paramName), paramName);
        if (string.IsNullOrEmpty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 参数名为空");
            return;
        }
        animator.SetBool(name, GetInputValue<bool>(nameof(value), value));
    }
}