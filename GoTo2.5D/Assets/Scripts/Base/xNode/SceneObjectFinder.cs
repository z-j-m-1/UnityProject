using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景物体按名字缓存查找器（All 来源用）：
/// 惰性构建"名字 → 物体"字典，把全场景 O(n) 扫描降为 O(1) 查表。
/// 失效：场景加载/卸载、运行时启动；编辑器 Hierarchy 变更即时失效；
/// 兜底：名字未命中时清缓存重扫一次再查（只在未命中时付出重扫成本）。
/// </summary>
public static class SceneObjectFinder
{
    private static Dictionary<string, GameObject> cache;
    private static bool built;
    private static bool dupWarned;

    /// <summary>标记缓存失效（场景结构变化时调用）</summary>
    public static void MarkDirty()
    {
        cache = null;
        built = false;
        dupWarned = false;
    }

    /// <summary>按名字找场景物体（含 inactive；重名取第一个并警告一次）</summary>
    public static GameObject Find(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (!built) Build();
        if (cache.TryGetValue(name, out GameObject go)) return go;

        // 未命中：场景结构可能变了，重扫一次兜底
        Build();
        return cache.TryGetValue(name, out go) ? go : null;
    }

    private static void Build()
    {
        Dictionary<string, GameObject> dict = new Dictionary<string, GameObject>();
        bool warnDup = false;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            GameObject[] roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                GameObject root = roots[r];
                if (root == null || string.IsNullOrEmpty(root.name)) continue;
                Add(dict, root, ref warnDup);

                Transform[] all = root.GetComponentsInChildren<Transform>(true);
                for (int c = 0; c < all.Length; c++)
                {
                    Transform child = all[c];
                    if (child != null && child != root.transform && !string.IsNullOrEmpty(child.name))
                    {
                        Add(dict, child.gameObject, ref warnDup);
                    }
                }
            }
        }

        cache = dict;
        built = true;

        if (warnDup && !dupWarned)
        {
            dupWarned = true;
            Debug.LogWarning("SceneObjectFinder: 场景中存在重名物体，All 查找只返回第一个匹配");
        }
    }

    private static void Add(Dictionary<string, GameObject> dict, GameObject go, ref bool warnDup)
    {
        if (dict.ContainsKey(go.name))
        {
            warnDup = true;
            return;
        }
        dict.Add(go.name, go);
    }

    static SceneObjectFinder()
    {
        SceneManager.sceneLoaded += (scene, mode) => MarkDirty();
        SceneManager.sceneUnloaded += scene => MarkDirty();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnRuntimeStart()
    {
        MarkDirty();
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void OnEditorLoad()
    {
        UnityEditor.EditorApplication.hierarchyChanged += MarkDirty;
    }
#endif
}