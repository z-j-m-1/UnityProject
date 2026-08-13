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

    [SerializeField] private VariableContainer<string> stringContainer = new VariableContainer<string>();
    [SerializeField] private VariableContainer<bool> boolContainer = new VariableContainer<bool>();
    [SerializeField] private VariableContainer<int> intContainer = new VariableContainer<int>();

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
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("变量名不能为空");
            return defaultValue;
        }

        Type type = typeof(T);

        if (type == typeof(string))
        {
            return (T)(object)stringContainer.Get(key, defaultValue as string);
        }
        if (type == typeof(bool))
        {
            return (T)(object)boolContainer.Get(key, (bool)(object)defaultValue);
        }
        if (type == typeof(int))
        {
            return (T)(object)intContainer.Get(key, (int)(object)defaultValue);
        }

        throw new NotSupportedException($"不支持的类型: {type}");
    }

    /// <summary>
    /// 设置变量值
    /// </summary>
    public void Set<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("变量名不能为空");
            return;
        }

        Type type = typeof(T);

        if (type == typeof(string))
        {
            stringContainer.Set(key, value as string);
            return;
        }
        if (type == typeof(bool))
        {
            boolContainer.Set(key, (bool)(object)value);
            return;
        }
        if (type == typeof(int))
        {
            intContainer.Set(key, (int)(object)value);
            return;
        }

        throw new NotSupportedException($"不支持的类型: {type}");
    }

    /// <summary>
    /// 检查变量是否存在
    /// </summary>
    public bool Has<T>(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;

        Type type = typeof(T);

        if (type == typeof(string))
        {
            return stringContainer.Has(key);
        }
        if (type == typeof(bool))
        {
            return boolContainer.Has(key);
        }
        if (type == typeof(int))
        {
            return intContainer.Has(key);
        }

        throw new NotSupportedException($"不支持的类型: {type}");
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

        stringContainer.Rebuild();
        boolContainer.Rebuild();
        intContainer.Rebuild();
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