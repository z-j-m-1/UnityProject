using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

/// <summary>
/// 节点浏览器数据：收藏 / 最近使用（EditorPrefs 持久化）+ 节点类型 ↔ 菜单路径工具
/// </summary>
public static class NodeBrowserPrefs
{
    private const string FavKey = "To2.5D.NodeBrowser.Favorites";
    private const string RecKey = "To2.5D.NodeBrowser.Recents";
    private const int MaxRecents = 12;

    [Serializable]
    private class StringList
    {
        public List<string> items = new List<string>();
    }

    private static List<KeyValuePair<string, Type>> _cache;

    public static List<string> GetFavorites()
    {
        return Load(FavKey);
    }

    public static List<string> GetRecents()
    {
        return Load(RecKey);
    }

    public static bool IsFavorite(string menu)
    {
        return GetFavorites().Contains(menu);
    }

    public static void ToggleFavorite(string menu)
    {
        List<string> fav = GetFavorites();
        if (fav.Contains(menu)) fav.Remove(menu);
        else fav.Insert(0, menu);
        Save(FavKey, fav);
    }

    public static void AddRecent(string menu)
    {
        if (string.IsNullOrEmpty(menu)) return;
        List<string> rec = GetRecents();
        rec.Remove(menu);
        rec.Insert(0, menu);
        if (rec.Count > MaxRecents) rec.RemoveRange(MaxRecents, rec.Count - MaxRecents);
        Save(RecKey, rec);
    }

    /// <summary>取节点菜单路径（与 NodeGraphEditor.GetNodeMenuName 同逻辑）</summary>
    public static string GetMenuName(Type type)
    {
        Node.CreateNodeMenuAttribute attrib;
        if (NodeEditorUtilities.GetAttrib(type, out attrib)) return attrib.menuName;
        return NodeEditorUtilities.NodeDefaultPath(type);
    }

    /// <summary>收集所有可用节点：菜单路径 → 类型（按路径排序，带缓存；域重载自动失效）</summary>
    public static List<KeyValuePair<string, Type>> CollectAllNodes()
    {
        if (_cache == null)
        {
            _cache = new List<KeyValuePair<string, Type>>();
            foreach (Type t in NodeEditorReflection.nodeTypes)
            {
                string path = GetMenuName(t);
                if (string.IsNullOrEmpty(path)) continue;
                _cache.Add(new KeyValuePair<string, Type>(path, t));
            }
            _cache.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
        }
        return _cache;
    }

    private static List<string> Load(string key)
    {
        try
        {
            StringList list = JsonUtility.FromJson<StringList>(EditorPrefs.GetString(key, "{}"));
            return list != null && list.items != null ? list.items : new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static void Save(string key, List<string> items)
    {
        EditorPrefs.SetString(key, JsonUtility.ToJson(new StringList { items = items }));
    }
}
