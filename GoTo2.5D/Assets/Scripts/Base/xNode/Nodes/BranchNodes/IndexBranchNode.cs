using UnityEngine;
using XNode;

/// <summary>
/// 流程分支-按索引分支：按 int 索引把链导向对应的分支端口；该分支未接线或索引越界时沿 next 兜底。
/// 典型用法：把「选项UI/获取选中索引」接到 index → 第 0 个分支 = 选第 0 项。
///
/// 分支端口是**动态端口列表**（面板里可增删排序），端口名形如 <c>branches 0</c>（★ 带空格，xNode 约定）。
/// 取值方式沿用 MultiBranchNode 已验证的写法：先查数组，再按端口名兜底。
/// </summary>
[CreateNodeMenu("流程分支/按索引分支")]
public class IndexBranchNode : FlowNode
{
    [Header("索引（接 选项UI/获取选中索引 等数据节点）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int index;

    [Header("分支（按索引对应；未接或越界走 next）")]
    [Output(dynamicPortList = true)]
    public BaseNode[] branches;

    public override BaseNode GetConnectedNode()
    {
        int i = GetInputValue<int>(nameof(index), index);

        // 方法 1：直接数组访问
        if (branches != null && i >= 0 && i < branches.Length && branches[i] != null)
        {
            return branches[i];
        }

        // 方法 2：按端口名取（★ 端口名是「字段名 序号」，中间有空格）
        NodePort port = GetPort($"{nameof(branches)} {i}");
        if (port != null && port.IsConnected)
        {
            NodePort connection = port.GetConnection(0);
            if (connection != null)
            {
                return connection.node as BaseNode;
            }
        }

        NodeLog.Warning($"{GetType().Name}: 索引 {i} 没有可用分支（越界或未接线），沿 next 继续");
        return base.GetConnectedNode();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(index))
        {
            return GetInputValue<int>(nameof(index), index);
        }
        return null;
    }
}
