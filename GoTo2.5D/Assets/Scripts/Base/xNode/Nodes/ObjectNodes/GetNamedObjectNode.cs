using UnityEngine;
using XNode;

/// <summary>
/// 取值-获取物体（名称）：在「图绑定物体」自身及其直接子物体里按名字查找。
/// 对象名称为 string 输入端口（可接线，未接线用字段值）；未命中返回 null。
/// 注意：只查一层子物体（transform.Find），不递归；要找全场景请用「取值/获取物体」（source=All）。
/// </summary>
[CreateNodeMenu("取值/获取物体(名称)")]
public class GetNamedObjectNode : GetObjectNodeBase
{
    [Header("对象名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string objectName;

    protected override GameObject Resolve()
    {
        string name = GetInputValue<string>(nameof(objectName), objectName);
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        GameObject attached = AttachedObject;
        if (attached == null)
        {
            return null;
        }

        // 名称等于图绑定物体自身也算命中（与「取值/获取物体」的 Self 模式一致）
        if (attached.name == name)
        {
            return attached;
        }

        Transform child = attached.transform.Find(name);
        if (child != null)
        {
            return child.gameObject;
        }

        NodeLog.Warning($"{GetType().Name}: 在 '{attached.name}' 自身及子物体中未找到名为 '{name}' 的物体");
        return null;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(objectName))
        {
            return GetInputValue<string>(nameof(objectName), objectName);
        }
        return base.GetValue(port);
    }
}
