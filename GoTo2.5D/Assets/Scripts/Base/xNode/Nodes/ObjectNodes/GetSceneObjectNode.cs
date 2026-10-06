using UnityEngine;
using UnityEngine.SceneManagement;
using XNode;

/// <summary>
/// 取值-获取物体（全场景）：按名字在**所有已加载场景**（含 DontDestroyOnLoad）里查找物体，
/// 含未激活物体，按层级顺序取第一个命中。
///
/// 实现刻意保持简单：每次求值现场扫描，不建缓存、不订阅场景事件。
/// ⚠️ 因此它是 O(场景物体数) 的：同帧/每帧高频读取（例如每帧驱动一个组件节点）会重复扫描，
/// 这种场景请改为「取值/获取物体(名称)」或把结果存进图变量后复用。
/// </summary>
[CreateNodeMenu("取值/获取物体(全场景)")]
public class GetSceneObjectNode : GetObjectNodeBase
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

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
            {
                continue;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                GameObject root = roots[r];
                if (root.name == name)
                {
                    return root;
                }

                Transform hit = FindInChildren(root.transform, name);
                if (hit != null)
                {
                    return hit.gameObject;
                }
            }
        }

        NodeLog.Warning($"{GetType().Name}: 全场景未找到名为 '{name}' 的物体");
        return null;
    }

    /// <summary>递归查找直接/间接子物体（含未激活）</summary>
    private static Transform FindInChildren(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
            {
                return child;
            }

            Transform hit = FindInChildren(child, name);
            if (hit != null)
            {
                return hit;
            }
        }
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
