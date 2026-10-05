using UnityEngine;
using XNode;

/// <summary>动画-浮点参数（Animator.SetFloat）</summary>
[CreateNodeMenu("动画/浮点参数")]
public class SetAnimatorFloatNode : ComponentActionNode<Animator>
{
    [Header("参数名（Float）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string paramName;

    [Header("值")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float value;

    protected override void Apply(Animator animator)
    {
        string name = GetInputValue<string>(nameof(paramName), paramName);
        if (string.IsNullOrEmpty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 参数名为空");
            return;
        }
        animator.SetFloat(name, GetInputValue<float>(nameof(value), value));
    }
}