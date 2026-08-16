using UnityEngine;

/// <summary>
/// 图参数发射器 - 挂场景物体：Inspector 里可视化编辑参数包，Emit() 时**带参数**触发事件（GraphEvent.data）。
/// 用法与 GraphEventEmitter 一致（按钮 / UnityEvent 拖入 Emit()），区别是本发射器同时把参数注入图内外部参数存储。
/// 自定义外部脚本也可直接声明 public GraphParamList xxx; 并在代码里调用 xxx.Build() 后传给执行器。
/// </summary>
public class GraphParamEmitter : MonoBehaviour
{
    [Tooltip("要触发的事件标识（对应图中入口节点的标识）")]
    public string eventId;

    [Tooltip("参数包（面板可视化编辑；Emit 时随事件注入图内外部参数存储）")]
    public GraphParamList parameters = new GraphParamList();

    /// <summary>给 UnityEvent / 按钮 onClick 拖入调用</summary>
    public void Emit()
    {
        GraphEvent.Trigger(e =>
        {
            e.eventId = eventId;
            e.data = parameters != null ? parameters.Build() : null;
        });
    }
}