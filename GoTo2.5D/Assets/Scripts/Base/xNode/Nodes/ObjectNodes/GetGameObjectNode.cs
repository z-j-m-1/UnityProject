using UnityEngine;
using XNode;

/// <summary>对象获取来源</summary>
public enum GameObjectSource
{
    /// <summary>图附加物体自身或子物体（transform.Find，层级查找）</summary>
    Self,
    /// <summary>全场景按名字查找（SceneObjectFinder 缓存字典，含 inactive）</summary>
    All
}

/// <summary>
/// 取值-获取物体：把目标物体作为数据输出，
/// 供操作节点（继承 ComponentActionNode）的 GameObject 输入端口接线。
/// 无序列化场景引用，运行时求值。
/// </summary>
[CreateNodeMenu("取值/获取物体")]
public class GetGameObjectNode : DataNode
{
    [Header("对象来源")]
    public GameObjectSource source = GameObjectSource.Self;

    [Header("对象名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string objectName;

    [Output]
    [System.NonSerialized]
    public GameObject output;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(objectName))
            return GetInputValue<string>(nameof(objectName), objectName);
        if (port.fieldName == nameof(output))
        {
            string name = GetInputValue<string>(nameof(objectName), objectName);
            output = Resolve(name);
            return output;
        }
        return null;
    }

    private GameObject Resolve(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        BaseNodeGraph nodeGraph = graph as BaseNodeGraph;
        switch (source)
        {
            case GameObjectSource.Self:
                GameObject attached = nodeGraph != null ? nodeGraph.GetAttachedObject() : null;
                if (attached == null) return null;
                if (attached.name == name) return attached;
                Transform t = attached.transform.Find(name);
                return t != null ? t.gameObject : null;

            case GameObjectSource.All:
                return SceneObjectFinder.Find(name);
        }
        return null;
    }
}