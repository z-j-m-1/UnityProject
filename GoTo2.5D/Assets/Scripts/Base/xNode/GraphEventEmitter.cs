using UnityEngine;

/// <summary>
/// 图事件发射器 - 挂场景物体，填 eventId
/// 给按钮 / UnityEvent 拖入 Emit()，触发 GraphEvent（无需引用节点图/执行器）
/// </summary>
public class GraphEventEmitter : MonoBehaviour
{
    [Tooltip("要触发的事件标识（对应图中入口节点的标识）")]
    public string eventId;

    /// <summary>给 UnityEvent / 按钮 onClick 拖入调用</summary>
    public void Emit()
    {
        GraphEvent.Trigger(e => e.eventId = eventId);
    }
}
