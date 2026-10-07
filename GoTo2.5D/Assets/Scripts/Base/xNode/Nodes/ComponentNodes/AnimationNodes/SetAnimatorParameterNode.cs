using UnityEngine;
using XNode;

/// <summary>
/// 动画-设置参数：**一个节点替代原来的「触发 / 布尔 / 浮点 / 整数参数」四个节点**。
/// 值类型由 <see cref="TypedValue"/> 的类型下拉选择：
///   None = Trigger（Trigger 只有名字、没有值）、Bool、Float、Int。
/// 参数名是输入端口（可接线）。
///
/// 这是 TypedValue（类型枚举 + 值联合）机制带来的合并 —— 原本四个同构节点只差值类型。
/// </summary>
[CreateNodeMenu("动画/设置参数")]
[NodeWidth(280)]
public class SetAnimatorParameterNode : ComponentActionNode<Animator>
{
    [Header("参数名")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string paramName;

    [Header("值（None = Trigger）")]
    public TypedValue value = new TypedValue();

    protected override void Apply(Animator animator)
    {
        string name = GetInputValue<string>(nameof(paramName), paramName);
        if (string.IsNullOrEmpty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 参数名为空");
            return;
        }

        switch (value.type)
        {
            case TypedValueType.None:
                animator.SetTrigger(name);
                NodeLog.Verbose($"{GetType().Name}: {name} = Trigger");
                break;

            case TypedValueType.Bool:
                animator.SetBool(name, value.boolValue);
                NodeLog.Verbose($"{GetType().Name}: {name} = {value.boolValue}");
                break;

            case TypedValueType.Float:
                animator.SetFloat(name, value.floatValue);
                NodeLog.Verbose($"{GetType().Name}: {name} = {value.floatValue}");
                break;

            case TypedValueType.Int:
                animator.SetInteger(name, value.intValue);
                NodeLog.Verbose($"{GetType().Name}: {name} = {value.intValue}");
                break;

            default:
                NodeLog.Warning(
                    $"{GetType().Name}: Animator 参数不支持类型 {value.TypeName}" +
                    "（只支持 None(Trigger) / Bool / Float / Int）");
                break;
        }
    }
}
