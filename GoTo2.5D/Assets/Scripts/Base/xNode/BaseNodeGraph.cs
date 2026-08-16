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

    /// <summary>收集图中已定义的全部变量名（跨类型，去重；Get/Set 节点编辑器下拉用）</summary>
    public System.Collections.Generic.List<string> GetAllVariableNames()
    {
        return variables.GetAllVariableNames();
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

    /// <summary>
    /// 获取当前生效的目标物体：优先取"正在执行节点的执行器"对象，其次回退 attachedObject（编辑模式 / 非执行上下文）
    /// </summary>
    public GameObject GetAttachedObject()
    {
        return NodeExecuteContext.Current != null ? NodeExecuteContext.Current.gameObject : attachedObject;
    }

    public void ResetGraph()
    {
        attachedObject = null;

        variables.Rebuild();
        invocationParams.Clear();
    }

    // ============ 图调用参数（瞬态：子图节点 / 外部代码 / 事件 / 状态机任一调用方触发图时注入；不序列化、不进存档） ============

    [NonSerialized] private Dictionary<string, object> invocationParams = new Dictionary<string, object>();

    /// <summary>设置调用参数（同名覆盖；由调用方在触发图时注入）</summary>
    public void SetInvocationParam(string name, object value)
    {
        if (string.IsNullOrEmpty(name)) return;
        invocationParams[name] = value;
    }

    /// <summary>读取调用参数；不存在或类型不匹配返回 fallback</summary>
    public T GetInvocationParam<T>(string name, T fallback = default)
    {
        if (!string.IsNullOrEmpty(name) && invocationParams.TryGetValue(name, out object value) && value is T typed)
        {
            return typed;
        }
        return fallback;
    }

    /// <summary>读取调用参数（不转型）；不存在返回 false</summary>
    public bool TryGetInvocationParam(string name, out object value)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return invocationParams.TryGetValue(name, out value);
        }
        value = null;
        return false;
    }

    /// <summary>移除指定调用参数</summary>
    public void ClearInvocationParam(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            invocationParams.Remove(name);
        }
    }

    /// <summary>清空全部调用参数</summary>
    public void ClearInvocationParams() => invocationParams.Clear();

    // ============ 调用参数兼容别名（旧命名：SetExternalParam 等，已废弃） ============

    [System.Obsolete("请使用 SetInvocationParam")]
    public void SetExternalParam(string name, object value) => SetInvocationParam(name, value);
    [System.Obsolete("请使用 GetInvocationParam")]
    public T GetExternalParam<T>(string name, T fallback = default) => GetInvocationParam(name, fallback);
    [System.Obsolete("请使用 TryGetInvocationParam")]
    public bool TryGetExternalParam(string name, out object value) => TryGetInvocationParam(name, out value);
    [System.Obsolete("请使用 ClearInvocationParam")]
    public void ClearExternalParam(string name) => ClearInvocationParam(name);
    [System.Obsolete("请使用 ClearInvocationParams")]
    public void ClearExternalParams() => ClearInvocationParams();

    /// <summary>读取图输出参数（图内「参数/输出」节点当前求值；外部代码执行后读取返回值用）</summary>
    public T GetOutputValue<T>(string paramName, T fallback = default)
    {
        if (string.IsNullOrEmpty(paramName) || nodes == null) return fallback;
        foreach (XNode.Node node in nodes)
        {
            if (node is SubGraphOutputNodeBase output && output.parameterName == paramName)
            {
                object v = output.EvaluateValue();
                if (v is T typed) return typed;
                return fallback;
            }
        }
        return fallback;
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