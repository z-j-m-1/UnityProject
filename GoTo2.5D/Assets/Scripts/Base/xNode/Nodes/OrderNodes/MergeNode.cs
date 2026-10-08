using XNode;

/// <summary>
/// 流程-集中：把**多条线**汇聚到同一个点，再从一条线继续往下 —— 用来避免"为了汇合而复制下游节点"。
///
/// **为什么这个类不需要任何代码**：xNode 的 <c>InputAttribute</c> 默认
/// <c>connectionType = ConnectionType.Multiple</c>（见 Node.cs 的构造函数默认参数），
/// 而 <see cref="FlowNode.input"/> 没有显式指定它，所以输入端**本来就能接多条连线**
/// —— 项目里那些"只能接一条"的值端口都显式写了 <c>ConnectionType.Override</c>，才被限制成单连接。
///
/// 所以它只承担两件事：给"集中"一个明确的菜单入口与名字，并在图上标出汇合点。
/// </summary>
[CreateNodeMenu("流程/集中")]
public class MergeNode : FlowNode
{
}
