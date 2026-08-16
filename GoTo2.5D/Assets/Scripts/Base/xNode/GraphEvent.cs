using ZGameFramework.Core;

/// <summary>
/// 图事件总线 - 通过字符串 eventId 驱动节点图入口
/// 任意触发源（按钮/碰撞/C# 代码）调用 GraphEvent.Trigger，执行器按入口标识订阅响应
/// </summary>
public class GraphEvent : ParameterizedEvent<GraphEvent>
{
    public string eventId;

    /// <summary>可选：触发时携带的外部参数（GraphExecutor 命中入口后注入图内外部参数存储）</summary>
    public GraphParams data;

    public override void OnRecycled()
    {
        eventId = null;
        data = null;
    }
}
