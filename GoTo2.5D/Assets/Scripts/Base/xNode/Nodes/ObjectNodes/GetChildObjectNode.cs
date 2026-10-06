using UnityEngine;
using XNode;

/// <summary>子物体选取方式</summary>
public enum ChildSelectMode
{
    /// <summary>按直接子物体索引（从 0 开始）</summary>
    ByIndex,

    /// <summary>按名称查找（可递归整棵子树）</summary>
    ByName
}

/// <summary>
/// 取值-获取物体（子物体）：取**任意物体**的直接子物体（按索引或按名称）。
/// 与「取值/获取物体(名称)」的区别：那个只在**图绑定物体**的层级里找，本节点可以取任何传入物体的子物体。
/// 只往下走一层（除非 recursive 递归）；含未激活物体。
/// </summary>
[CreateNodeMenu("取值/获取物体(子物体)")]
public class GetChildObjectNode : GetObjectNodeBase
{
    [Header("父物体")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject parent;

    [Header("选取方式")]
    public ChildSelectMode mode = ChildSelectMode.ByIndex;

    [Header("索引（从 0 开始）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int index;

    [Header("名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string childName;

    [Header("按名称递归查找整棵子树")]
    public bool recursive = false;

    protected override GameObject Resolve()
    {
        GameObject parentObj = GetInputValue<GameObject>(nameof(parent), null);
        if (parentObj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入父物体（请把「取值/获取物体」接到「父物体」端口）");
            return null;
        }

        Transform t = parentObj.transform;

        if (mode == ChildSelectMode.ByIndex)
        {
            int i = GetInputValue<int>(nameof(index), index);
            if (i < 0 || i >= t.childCount)
            {
                NodeLog.Warning($"{GetType().Name}: '{parentObj.name}' 的子物体索引 {i} 越界（共 {t.childCount} 个）");
                return null;
            }
            return t.GetChild(i).gameObject;
        }

        string name = GetInputValue<string>(nameof(childName), childName);
        if (string.IsNullOrEmpty(name))
        {
            NodeLog.Warning($"{GetType().Name}: 子物体名称为空");
            return null;
        }

        Transform hit = FindChild(t, name, recursive);
        if (hit == null)
        {
            NodeLog.Warning($"{GetType().Name}: 在 '{parentObj.name}' 的{(recursive ? "子树" : "直接子物体")}中未找到名为 '{name}' 的物体");
        }
        return hit != null ? hit.gameObject : null;
    }

    /// <summary>按名查找子物体（含未激活）；deep = true 时递归整棵子树</summary>
    private static Transform FindChild(Transform parent, string name, bool deep)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
            {
                return child;
            }

            if (deep)
            {
                Transform hit = FindChild(child, name, true);
                if (hit != null)
                {
                    return hit;
                }
            }
        }
        return null;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(parent)) return GetInputValue<GameObject>(nameof(parent), null);
        if (port.fieldName == nameof(index)) return GetInputValue<int>(nameof(index), index);
        if (port.fieldName == nameof(childName)) return GetInputValue<string>(nameof(childName), childName);
        return base.GetValue(port);
    }
}
