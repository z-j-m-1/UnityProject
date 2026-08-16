using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;
#if UNITY_2019_1_OR_NEWER && USE_ADVANCED_GENERIC_MENU
using GenericMenu = XNodeEditor.AdvancedGenericMenu;
#endif

/// <summary>
/// BaseNodeGraph 图编辑器扩展：
/// - 空白处右键菜单顶部插入「★ 收藏」「最近使用」「打开节点浏览器…」
/// - 任意方式创建节点都自动记入「最近使用」
/// </summary>
[CustomNodeGraphEditor(typeof(BaseNodeGraph))]
public class BaseNodeGraphEditor : NodeGraphEditor
{
    public override void AddContextMenuItems(GenericMenu menu, Type compatibleType = null, NodePort.IO direction = NodePort.IO.Input)
    {
        if (compatibleType == null)
        {
            Vector2 pos = NodeEditorWindow.current != null
                ? NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition)
                : Vector2.zero;
            AddQuickItems(menu, pos);
        }
        base.AddContextMenuItems(menu, compatibleType, direction);
    }

    private void AddQuickItems(GenericMenu menu, Vector2 pos)
    {
        List<KeyValuePair<string, Type>> all = NodeBrowserPrefs.CollectAllNodes();

        List<string> favs = NodeBrowserPrefs.GetFavorites();
        List<string> validFavs = new List<string>();
        foreach (var kv in all) if (favs.Contains(kv.Key)) validFavs.Add(kv.Key);
        if (validFavs.Count > 0)
        {
            menu.AddDisabledItem(new GUIContent("★ 收藏"));
            foreach (string path in validFavs)
            {
                menu.AddItem(new GUIContent("★ 收藏/" + path), false, () => CreateAt(path, pos));
            }
        }

        List<string> recs = NodeBrowserPrefs.GetRecents();
        List<string> validRecs = new List<string>();
        foreach (var kv in all) if (recs.Contains(kv.Key)) validRecs.Add(kv.Key);
        if (validRecs.Count > 0)
        {
            menu.AddDisabledItem(new GUIContent("最近使用"));
            foreach (string path in validRecs)
            {
                menu.AddItem(new GUIContent("最近使用/" + path), false, () => CreateAt(path, pos));
            }
        }

        menu.AddItem(new GUIContent("打开节点浏览器…"), false, () => NodeBrowserWindow.Open(pos));
        menu.AddSeparator("");
    }

    private void CreateAt(string path, Vector2 pos)
    {
        foreach (var kv in NodeBrowserPrefs.CollectAllNodes())
        {
            if (kv.Key == path)
            {
                XNode.Node node = CreateNode(kv.Value, pos);
                if (node != null && NodeEditorWindow.current != null) NodeEditorWindow.current.AutoConnect(node);
                return;
            }
        }
    }

    public override XNode.Node CreateNode(Type type, Vector2 position)
    {
        XNode.Node node = base.CreateNode(type, position);
        if (node != null) NodeBrowserPrefs.AddRecent(NodeBrowserPrefs.GetMenuName(type));
        return node;
    }
}
