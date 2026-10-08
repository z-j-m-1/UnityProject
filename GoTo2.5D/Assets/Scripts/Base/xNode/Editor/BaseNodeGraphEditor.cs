using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    // ============ 拖拽场景物体 → 生成「取值/获取物体(全场景)」节点 ============

    /// <summary>
    /// 图编辑器的拖放处理（两种）：
    ///   ① **拖入 .cs 脚本** → 建一个类型已填好的「组件/添加·移除」节点；
    ///   ② **拖入场景物体**（Hierarchy）→ 为每个物体生成一个「取值/获取物体(全场景)」节点，`objectName` = 物体名。
    ///
    /// 由 xNode 的 NodeEditorAction 在 DragPerform 时调用（见 NodeGraphEditor.OnDropObjects）。
    /// 其它资源（预制体等）不处理，不抢别的拖放逻辑。
    ///
    /// 注：拖脚本能"建节点并填好类型名"，但节点**存的仍是字符串** ——
    /// <c>MonoScript</c> 是 UnityEditor 类型，运行期节点不能持有它（打包会编译不过）。
    /// </summary>
    public override void OnDropObjects(UnityEngine.Object[] objects)
    {
        if (objects == null || objects.Length == 0) return;

        Vector2 pos = NodeEditorWindow.current != null
            ? NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition)
            : Vector2.zero;

        int created = 0;
        foreach (UnityEngine.Object o in objects)
        {
            // ① 拖入 .cs 脚本 → 直接建好「组件/添加·移除」节点并填上类型
            if (TryHandleScriptDrop(o, ref pos, ref created)) continue;

            // ② 拖入场景物体 → 建「取值/获取物体(全场景)」节点
            GameObject go = ResolveSceneGameObject(o);
            if (go == null) continue;

            XNode.Node node = CreateNode(typeof(GetSceneObjectNode), pos);
            GetSceneObjectNode getter = node as GetSceneObjectNode;
            if (getter == null) continue;

            Undo.RecordObject(getter, "Drop Scene Object");
            getter.objectName = go.name;
            EditorUtility.SetDirty(getter);

            WarnIfDuplicateName(go);

            created++;
            pos += new Vector2(0f, 130f);   // 多选拖拽时竖向错开，避免节点叠在一起
        }

        if (created == 0) return;

        EditorUtility.SetDirty(target);   // 节点是图资产的子资产，确保图资产被标记为已修改
        if (NodeEditorPreferences.GetSettings().autoSave) AssetDatabase.SaveAssets();
        NodeEditorWindow.RepaintAll();
    }

    /// <summary>
    /// 拖入的是 .cs 脚本时：建一个「组件/添加·移除」节点并把类型全名填好。
    /// 返回 true = 已接管本次拖放（**包括**"是脚本但不是组件脚本"的情况，避免它再落到场景物体分支）。
    /// </summary>
    private bool TryHandleScriptDrop(UnityEngine.Object o, ref Vector2 pos, ref int created)
    {
        MonoScript script = o as MonoScript;
        if (script == null) return false;

        Type scriptType = script.GetClass();
        if (scriptType == null || !typeof(Component).IsAssignableFrom(scriptType))
        {
            return true;   // 是脚本但不是组件（例如纯工具类 / 节点脚本），忽略
        }

        XNode.Node node = CreateNode(typeof(ModifyComponentNode), pos);
        ModifyComponentNode modify = node as ModifyComponentNode;
        if (modify == null) return true;

        Undo.RecordObject(modify, "Drop Script");
        modify.componentTypeName = scriptType.FullName;
        EditorUtility.SetDirty(modify);

        created++;
        pos += new Vector2(0f, 130f);   // 多选拖拽时竖向错开
        return true;
    }

    /// <summary>把拖入对象解析成场景里的 GameObject（兼容拖 Component / Transform）；资源返回 null</summary>
    private static GameObject ResolveSceneGameObject(UnityEngine.Object o)
    {
        if (o == null) return null;

        GameObject go = o as GameObject;
        if (go == null)
        {
            Component c = o as Component;
            if (c != null) go = c.gameObject;
        }
        if (go == null) return null;

        return EditorUtility.IsPersistent(go) ? null : go;
    }

    /// <summary>
    /// 同名物体警告：「全场景」查找按层级顺序取第一个，同名时可能不是拖进来的这一个。
    /// 静默指向错物体最难查，所以这里主动提示，并给出拖入物体的层级路径便于区分。
    /// </summary>
    private static void WarnIfDuplicateName(GameObject dragged)
    {
        int count = 0;
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < all.Length; i++)
        {
            GameObject g = all[i];
            if (g == null || EditorUtility.IsPersistent(g)) continue;   // 排除资产（预制体等）
            if (g.name == dragged.name) count++;
        }

        if (count <= 1) return;

        Debug.LogWarning(
            $"节点图：场景里有 {count} 个名为 '{dragged.name}' 的物体；" +
            $"「取值/获取物体(全场景)」按层级顺序取第一个，可能不是拖进来的这个" +
            $"（拖入的是：{GetHierarchyPath(dragged)}）。请在节点上确认；" +
            $"若必须精确指向它，请用「参数/输入/物体」+ 场景侧 GraphParamEmitter" +
            $"（「引用」节点只能装资产，装不了场景物体）。",
            dragged);
    }

    /// <summary>取物体的层级路径（A/B/C），便于在警告里区分同名物体</summary>
    private static string GetHierarchyPath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }
}
