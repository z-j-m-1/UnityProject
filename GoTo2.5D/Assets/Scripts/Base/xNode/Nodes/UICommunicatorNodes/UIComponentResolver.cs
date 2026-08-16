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
            if (Application.isPlaying)
            {
                return UICollector.Instance.Find<T>(uiObjectName);
            }
#if UNITY_EDITOR
            // 编辑器模式：UICollector 的 Awake 不触发（不会收集），直接扫场景 Canvas 下组件
            // 纯只读：不创建 Canvas、不新增组件、不修改场景
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return null;
            T[] comps = canvas.GetComponentsInChildren<T>(true);
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i].gameObject.name == uiObjectName) return comps[i];
            }
            return null;
#else
            return null;
#endif
        }

        // Self：自身或子物体（按当前执行器解析目标，避免多执行器共享 attachedObject 互相覆盖）
        GameObject attached = graph != null ? graph.GetAttachedObject() : null;
        if (attached != null)
        {
            if (attached.name == uiObjectName)
            {
                return attached.GetComponent<T>();
            }
            Transform t = attached.transform.Find(uiObjectName);
            if (t != null)
            {
                return t.GetComponent<T>();
            }
        }
        return null;
    }
}
