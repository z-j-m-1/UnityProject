using UnityEngine;
using XNode;

/// <summary>动画-触发参数（Animator.SetTrigger）</summary>
[CreateNodeMenu("动画/触发参数")]
public class SetAnimatorTriggerNode : ComponentActionNode<Animator>
{
    [Header("参数名（Trigger）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string triggerName;

    protected override void Apply(Animator animator)
    {
        string name = GetInputValue<string>(nameof(triggerName), triggerName);
        if (string.IsNullOrEmpty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 参数名为空");
            return;
        }
        animator.SetTrigger(name);
    }
}