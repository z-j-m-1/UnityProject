using UnityEngine;
using XNode;

/// <summary>
/// 获取物体节点基类（数据节点，不参与流程）：统一「输出一个 GameObject 数据端口」+ 运行时求值，
/// 子类只实现 Resolve() 决定从哪里拿到物体。对应组件节点原本的三种目标模式：
///   Attached（自身） → GetSelfObjectNode
///   ByName （名称） → GetNamedObjectNode
///   Direct （引用） → GetReferencedObjectNode
///
/// 输出口非序列化（运行时求值，规避场景引用写进图资产 / 跨场景重载失效）；
/// 非序列化端口由 VisiblePortsNodeEditor 补画，基类注册一次即可覆盖全部子类。
/// </summary>
public abstract class GetObjectNodeBase : DataNode
{
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject output;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName != nameof(output))
        {
            return null;
        }

        output = Resolve();
        return output;
    }

    /// <summary>子类实现：解析出目标物体（取不到返回 null）</summary>
    protected abstract GameObject Resolve();

    /// <summary>
    /// 图绑定物体：执行期间 = 当前正在跑的执行器所在物体（NodeExecuteContext.Current）；
    /// 非执行期（编辑器 / 图外调用）回退图资产上的 attachedObject。
    /// </summary>
    protected GameObject AttachedObject
    {
        get
        {
            BaseNodeGraph nodeGraph = this.graph as BaseNodeGraph;
            return nodeGraph != null ? nodeGraph.GetAttachedObject() : null;
        }
    }
}
