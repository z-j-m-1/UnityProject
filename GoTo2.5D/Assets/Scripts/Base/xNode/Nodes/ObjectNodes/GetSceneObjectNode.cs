using UnityEngine;
using UnityEngine.SceneManagement;
using XNode;

/// <summary>
/// 取值-获取物体（全场景）：按名字在**所有已加载场景**（含 DontDestroyOnLoad）里查找物体，
/// 含未激活物体，按层级顺序取第一个命中。
///
/// 带缓存（反复读取同一个物体时只扫一次）：
///   · 键 = 对象名称（objectName 可接线，名称一变缓存即作废）；
///   · 命中后直接返回；物体被销毁时 Unity 的 fake-null 会让它落到重扫分支 —— **自愈，无需任何失效订阅**；
///   · "没找到"不写负缓存，所以延迟创建的物体会在下次读取时被找到（代价：找不到时每次仍全扫，故警告每个名字只打一次）。
///
/// 本节点**可以安全缓存**，因为结果只由名称决定、**与执行宿主无关** ——
/// 多执行器 / 多链读到的是同一个物体；两条链用不同名字互相冲掉缓存也只是退化成"重扫"，绝不会返回错物体。
/// （对比「取值/获取物体(名称)」：其结果依赖隐藏的"图绑定物体"，缓存必须把宿主也算进键，否则会返回别的执行器的物体。）
///
/// 注意：缓存后**不做名字复核** —— 目标物体即使被改名，本节点仍返回原来那个（"稳定引用"语义）。
/// 若需要严格按名匹配，在命中分支加一次 `cached.name == name` 校验即可。
/// </summary>
[CreateNodeMenu("取值/获取物体(全场景)")]
public class GetSceneObjectNode : GetObjectNodeBase
{
    [Header("对象名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string objectName;

    private GameObject cached;
    private string cachedName;
    private string warnedName;   // 同一名字只警告一次，避免下游逐帧读取时刷日志

    protected override GameObject Resolve()
    {
        string name = GetInputValue<string>(nameof(objectName), objectName);
        if (string.IsNullOrEmpty(name))
        {
            cached = null;
            cachedName = null;
            return null;
        }

        // 名称变了（端口可接线）→ 缓存作废
        if (cachedName != name)
        {
            cachedName = name;
            cached = null;
        }

        // 命中缓存。物体被销毁时 cached == null（Unity fake-null）→ 自动落到下面重扫，所以不需要失效机制
        if (cached != null)
        {
            return cached;
        }

        cached = FindInScene(name);
        if (cached == null && warnedName != name)
        {
            warnedName = name;
            NodeLog.Warning($"{GetType().Name}: 全场景未找到名为 '{name}' 的物体（物体延迟创建时下次读取会自动重试）");
        }
        return cached;
    }

    /// <summary>现场扫描：所有已加载场景的根物体及其子孙（含未激活），按层级顺序取第一个命中</summary>
    private static GameObject FindInScene(string name)
    {
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
