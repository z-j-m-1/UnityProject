using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

/// <summary>
/// 节点浏览器：搜索 / 最近使用 / 收藏，点击即把节点加到当前打开的节点图
/// 入口：节点图空白处右键 → 打开节点浏览器…；或 Tools/节点系统/节点浏览器
/// </summary>
public class NodeBrowserWindow : EditorWindow
{
    private string search = "";
    private Vector2 scroll;
    private Vector2 addPosition;
    private List<KeyValuePair<string, Type>> allNodes = new List<KeyValuePair<string, Type>>();
    private List<KeyValuePair<string, Type>> favorites = new List<KeyValuePair<string, Type>>();
    private List<KeyValuePair<string, Type>> recents = new List<KeyValuePair<string, Type>>();

    public static void OpenBrowser()
    {
        Vector2 gridPos = Vector2.zero;
        NodeEditorWindow win = NodeEditorWindow.current;
        if (win != null)
        {
            gridPos = win.WindowToGridPosition(new Vector2(win.position.width * 0.5f, win.position.height * 0.5f));
        }
        Open(gridPos);
    }

    public static void Open(Vector2 gridPosition)
    {
        NodeBrowserWindow w = GetWindow<NodeBrowserWindow>(true, "节点浏览器");
        w.addPosition = gridPosition;
        w.minSize = new Vector2(340, 440);
        w.Refresh();
        w.Show();
    }

    [MenuItem("Tools/节点系统/节点浏览器")]
    private static void MenuOpen()
    {
        OpenBrowser();
    }

    private void Refresh()
    {
        allNodes = NodeBrowserPrefs.CollectAllNodes();
        Dictionary<string, Type> map = new Dictionary<string, Type>();
        foreach (var kv in allNodes) map[kv.Key] = kv.Value;

        favorites.Clear();
        foreach (string p in NodeBrowserPrefs.GetFavorites())
            if (map.TryGetValue(p, out Type t)) favorites.Add(new KeyValuePair<string, Type>(p, t));

        recents.Clear();
        foreach (string p in NodeBrowserPrefs.GetRecents())
            if (map.TryGetValue(p, out Type t)) recents.Add(new KeyValuePair<string, Type>(p, t));
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();
        GUI.SetNextControlName("NodeBrowserSearch");
        string newSearch = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
        if (newSearch != search)
        {
            search = newSearch;
        }
        if (GUILayout.Button("清空", EditorStyles.miniButton, GUILayout.Width(46)))
        {
            search = "";
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (string.IsNullOrWhiteSpace(search))
        {
            DrawSection("★ 收藏", favorites, true);
            DrawSection("最近使用", recents, false);
            DrawAllGrouped();
        }
        else
        {
            DrawSearchResults();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawSection(string header, List<KeyValuePair<string, Type>> items, bool starable)
    {
        if (items.Count == 0) return;
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
        foreach (var kv in items)
        {
            DrawItem(kv.Key, kv.Value, starable);
        }
        EditorGUILayout.Space(4);
    }

    private void DrawItem(string path, Type type, bool starable)
    {
        EditorGUILayout.BeginHorizontal();
        if (starable && GUILayout.Button(NodeBrowserPrefs.IsFavorite(path) ? "★" : "☆", EditorStyles.miniButton, GUILayout.Width(24)))
        {
            NodeBrowserPrefs.ToggleFavorite(path);
            Refresh();
            return;
        }
        if (GUILayout.Button(path, GUILayout.Height(20)))
        {
            AddNode(path, type);
            return;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawAllGrouped()
    {
        EditorGUILayout.LabelField("全部（" + allNodes.Count + "）", EditorStyles.boldLabel);
        string lastCategory = null;
        foreach (var kv in allNodes)
        {
            string path = kv.Key;
            int slash = path.IndexOf('/');
            string category = slash >= 0 ? path.Substring(0, slash) : "（其他）";
            string label = slash >= 0 ? path.Substring(slash + 1) : path;
            if (category != lastCategory)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField(category, EditorStyles.miniBoldLabel);
                lastCategory = category;
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(NodeBrowserPrefs.IsFavorite(path) ? "★" : "☆", EditorStyles.miniButton, GUILayout.Width(24)))
            {
                NodeBrowserPrefs.ToggleFavorite(path);
                Refresh();
                return;
            }
            if (GUILayout.Button(label, GUILayout.Height(20)))
            {
                AddNode(path, kv.Value);
                return;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSearchResults()
    {
        string q = search.Trim().ToLowerInvariant();
        int shown = 0;
        foreach (var kv in allNodes)
        {
            string path = kv.Key;
            if (path.ToLowerInvariant().Contains(q) || kv.Value.Name.ToLowerInvariant().Contains(q))
            {
                shown++;
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(NodeBrowserPrefs.IsFavorite(path) ? "★" : "☆", EditorStyles.miniButton, GUILayout.Width(24)))
                {
                    NodeBrowserPrefs.ToggleFavorite(path);
                    Refresh();
                    return;
                }
                if (GUILayout.Button(path, GUILayout.Height(20)))
                {
                    AddNode(path, kv.Value);
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        if (shown == 0) EditorGUILayout.HelpBox("没有匹配的节点", MessageType.Info);
    }

    private void AddNode(string path, Type type)
    {
        NodeEditorWindow win = NodeEditorWindow.current;
        if (win == null || win.graph == null)
        {
            EditorUtility.DisplayDialog("节点浏览器", "没有打开的节点图，请先打开一张图（双击图资产）", "确定");
            return;
        }
        NodeGraphEditor ge = NodeGraphEditor.GetEditor(win.graph, win);
        XNode.Node node = ge.CreateNode(type, addPosition);
        if (node != null) win.AutoConnect(node);
        NodeBrowserPrefs.AddRecent(path);
        Close();
    }
}

