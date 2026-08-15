using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu(menuName = "节点图/通用", fileName = "通用节点图")]
public class BaseNodeGraph : NodeGraph, ISerializationCallbackReceiver
{
    // 节点图依附的对象
    [NonSerialized]
    public GameObject attachedObject;

    [SerializeField]
    public StartNode startNode;

    [NonSerialized]
    private BaseNode _currentNode;

    // ============ 节点图唯一标识（存档用） ============

    [SerializeField] private string _guid;

    /// <summary>
    /// 节点图的稳定唯一标识（无则自动生成）
    /// </summary>
    public string Guid
    {
        get
        {
            EnsureGuid();
            return _guid;
        }
    }

    private void EnsureGuid()
    {
        if (string.IsNullOrEmpty(_guid))
        {
            _guid = System.Guid.NewGuid().ToString();
        }
    }

    public void OnBeforeSerialize()
    {
        EnsureGuid();
    }

    public void OnAfterDeserialize()
    {
        EnsureGuid();
    }

    /// <summary>
    /// 重新生成 GUID（仅当你确实需要更换标识时使用）
    /// </summary>
    [ContextMenu("重新生成GUID")]
    public void RegenerateGuid()
    {
        _guid = System.Guid.NewGuid().ToString();
        Debug.Log($"节点图 '{name}' 已重新生成 GUID: {_guid}");
    }

    // ============ 变量容器 ============

    [SerializeField] private VariableBundle variables = new VariableBundle();

    // ============ 公共属性 ============

    public BaseNode CurrentNode
    {
        get { return _currentNode; }
        set { _currentNode = value; }
    }

    // ============ 公共泛型 API ============

    /// <summary>
    /// 获取变量值
    /// </summary>
    public T Get<T>(string key, T defaultValue = default)
    {
        return variables.Get(key, defaultValue);
    }

    /// <summary>
    /// 设置变量值
    /// </summary>
    public void Set<T>(string key, T value)
    {
        variables.Set(key, value);
    }

    /// <summary>
    /// 检查变量是否存在
    /// </summary>
    public bool Has<T>(string key)
    {
        return variables.Has<T>(key);
    }

    /// <summary>
    /// 从存档导入图变量
    /// </summary>
    public void ImportVariables(VariableBundleData data)
    {
        variables.ImportFrom(data);
    }

    /// <summary>
    /// 导出图变量到存档
    /// </summary>
    public VariableBundleData ExportVariables()
    {
        return variables.Export();
    }

    /// <summary>
    /// 名字优先 + GUID 兜底获取变量（得到实际名字与 GUID）
    /// </summary>
    public bool TryGetVariable<T>(string name, string guid, out T value, out string actualName, out string actualGuid)
    {
        return variables.TryResolve(name, guid, out value, out actualName, out actualGuid);
    }

    /// <summary>
    /// 名字优先 + GUID 兜底设置变量
    /// </summary>
    public bool TrySetVariable<T>(string name, string guid, T value, out string actualName, out string actualGuid)
    {
        return variables.TryResolveAndSet(name, guid, value, out actualName, out actualGuid);
    }

    // ============ 节点图生命周期方法 ============

    public void SetAttachedObject(GameObject obj)
    {
        attachedObject = obj;
    }

    public void ResetGraph()
    {
        _currentNode = null;
        attachedObject = null;

        variables.Rebuild();
    }

    // ============ 入口节点 ============

    /// <summary>
    /// 按标识符（名字优先）/ GUID 兜底查找入口节点；找不到返回 null
    /// 运行时与编辑器均实时扫描 nodes，动态变更也能命中
    /// </summary>
    public EntryNode GetEntryNode(string id)
    {
        if (string.IsNullOrEmpty(id) || nodes == null) return null;

        EntryNode guidMatch = null;
        foreach (Node node in nodes)
        {
            if (node is EntryNode entry)
            {
                // 名字优先
                if (!string.IsNullOrEmpty(entry.Identifier) && entry.Identifier == id)
                {
                    return entry;
                }
                // GUID 兜底（记住第一个匹配）
                if (guidMatch == null && !string.IsNullOrEmpty(entry.Guid) && entry.Guid == id)
                {
                    guidMatch = entry;
                }
            }
        }
        return guidMatch;
    }

    /// <summary>
    /// 获取图中所有入口节点（按节点列表顺序）
    /// </summary>
    public List<EntryNode> GetAllEntryNodes()
    {
        List<EntryNode> entries = new List<EntryNode>();
        if (nodes == null) return entries;
        foreach (Node node in nodes)
        {
            if (node is EntryNode entry)
            {
                entries.Add(entry);
            }
        }
        return entries;
    }

    // ============ XNode 重写 ============

    public override Node AddNode(Type type)
    {
        Node node = base.AddNode(type);

        if (node is StartNode startNodeComponent)
        {
            if (startNode != null && startNode != startNodeComponent)
            {
                Debug.LogWarning($"节点图 '{name}' 中已存在 StartNode，将替换为新的");
            }
            startNode = startNodeComponent;
            Debug.Log($"节点图 '{name}' 已设置 StartNode: {startNodeComponent.name}");
        }

        return node;
    }
}