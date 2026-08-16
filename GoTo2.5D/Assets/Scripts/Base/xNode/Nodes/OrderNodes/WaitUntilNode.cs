using XNode;

/// <summary>
/// 条件等待-通用：等待 condition 输入端口为 true
/// 可由比较 / 逻辑 / 变量等数据节点组合出任意条件
/// </summary>
[CreateNodeMenu("流程/等待条件")]
public class WaitUntilNode : ConditionWaitNode
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public bool condition;

    protected override bool CheckCondition()
    {
        return GetInputValue<bool>(nameof(condition), condition);
    }
}
