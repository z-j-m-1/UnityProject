using UnityEngine;

/// <summary>
/// 碰撞事件发射器 - 挂带 Collider 的触发器物体，进入时触发 GraphEvent（2D/3D 都支持）
/// </summary>
public class CollisionEventEmitter : MonoBehaviour
{
    [Tooltip("要触发的事件标识（对应图中入口节点的标识）")]
    public string eventId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GraphEvent.Trigger(e => e.eventId = eventId);
    }

    private void OnTriggerEnter(Collider other)
    {
        GraphEvent.Trigger(e => e.eventId = eventId);
    }
}
