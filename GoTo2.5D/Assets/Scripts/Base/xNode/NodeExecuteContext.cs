/// <summary>
/// 节点执行上下文：记录当前正在执行节点的 GraphExecutor
/// 用于节点按"当前执行器"解析目标（如 attachedObject），避免图资产上的共享状态被多执行器互相覆盖
/// </summary>
public static class NodeExecuteContext
{
    /// <summary>当前正在执行节点的执行器（Execute 期间有效，其余时间为 null）</summary>
    public static GraphExecutor Current;
}
