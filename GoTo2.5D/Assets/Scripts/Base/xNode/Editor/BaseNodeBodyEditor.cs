#if UNITY_EDITOR
using UnityEditor;
using XNode;
using XNodeEditor;

/// <summary>
/// 所有 BaseNode 的默认节点体编辑器：**继承 <see cref="VisiblePortsNodeEditor"/>**，
/// 因此自动获得"补画 [NonSerialized] 字段端口"的能力（Unity 的序列化迭代器不含非序列化字段，
/// 默认编辑器会漏掉 GameObject 等非序列化端口），并额外包一层运行时高亮。
///
/// 好处：注册在 <see cref="BaseNode"/> 上一次即可覆盖**全部**节点 ——
/// 新增带非序列化端口的节点**不再需要单独注册**（以前每个节点族都要写一行
/// <c>[CustomNodeEditor(typeof(X))] : VisiblePortsNodeEditor</c>，漏了端口就不显示）。
/// </summary>
[CustomNodeEditor(typeof(BaseNode))]
public class BaseNodeBodyEditor : VisiblePortsNodeEditor
{
    public override void OnBodyGUI()
    {
        BaseNode baseNode = target as BaseNode;
        bool highlighted = NodeRunHighlight.BeginIfActive(baseNode);

        base.OnBodyGUI();   // VisiblePortsNodeEditor：序列化属性 + 动态端口 + 非序列化端口

        if (highlighted)
        {
            NodeRunHighlight.EndHighlight();
        }
    }
}
#endif
