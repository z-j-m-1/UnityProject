using UnityEngine;

/// <summary>
/// 按来源解析 UI 组件
/// Self：图附加物体的自身或子物体；Canvas：UI 收集者按名字查找
/// </summary>
public static class UIComponentResolver
{
    public static T Resolve<T>(UISource source, string uiObjectName, BaseNodeGraph graph) where T : Component
    {
        if (string.IsNullOrEmpty(uiObjectName)) return null;

        if (source == UISource.Canvas)
        {
            return UICollector.Instance.Find<T>(uiObjectName);
        }

        // Self：自身或子物体
        if (graph != null && graph.attachedObject != null)
        {
            if (graph.attachedObject.name == uiObjectName)
            {
                return graph.attachedObject.GetComponent<T>();
            }
            Transform t = graph.attachedObject.transform.Find(uiObjectName);
            if (t != null)
            {
                return t.GetComponent<T>();
            }
        }
        return null;
    }
}
