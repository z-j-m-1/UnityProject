using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu(menuName = "节点图/通用", fileName = "通用节点图")]
public class BaseNodeGraph : NodeGraph
{
    // 节点图依附的对象
    [NonSerialized]
    public GameObject attachedObject;

    [SerializeField]
    public StartNode startNode;

    [NonSerialized]
    private BaseNode _currentNode;

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