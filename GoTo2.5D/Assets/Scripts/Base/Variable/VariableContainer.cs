using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 变量容器 - 一个泛型参数
/// </summary>
/// <typeparam name="T">变量值类型</typeparam>
[Serializable]
public class VariableContainer<T>
{
    [SerializeField] private List<Variable<T>> variableList = new List<Variable<T>>();

    [NonSerialized] private Dictionary<string, T> runtimeDict;

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器模式下直接从列表构建字典（每次重新构建）
    /// </summary>
    private Dictionary<string, T> GetRuntimeDict()
    {
        // 编辑器模式下每次都重新构建，确保读取最新的列表数据
        BuildRuntimeDict();
        return runtimeDict;
    }
#else
    /// <summary>
    /// 运行时使用缓存字典（懒加载）
    /// </summary>
    private Dictionary<string, T> GetRuntimeDict()
    {
        if (runtimeDict == null)
        {
            BuildRuntimeDict();
        }
        return runtimeDict;
    }
#endif

    /// <summary>
    /// 从列表构建字典
    /// </summary>
    private void BuildRuntimeDict()
    {
        runtimeDict = new Dictionary<string, T>();

        foreach (var variable in variableList)
        {
            if (variable != null && !string.IsNullOrEmpty(variable.Name))
            {
                if (!runtimeDict.ContainsKey(variable.Name))
                {
                    runtimeDict.Add(variable.Name, variable.Value);
                }
                else
                {
                    Debug.LogWarning($"变量容器中存在重复的变量名: {variable.Name}");
                }
            }
        }
    }

    /// <summary>
    /// 重建字典
    /// </summary>
    public void Rebuild()
    {
        runtimeDict = null;
#if !UNITY_EDITOR
        GetRuntimeDict();
#endif
    }

    /// <summary>
    /// 获取变量值
    /// </summary>
    public T Get(string key, T defaultValue = default)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("变量名不能为空");
            return defaultValue;
        }

        var dict = GetRuntimeDict();
        if (dict.TryGetValue(key, out T value))
        {
            Debug.Log($"获取变量: {key} = {value}");
            return value;
        }
        Debug.LogWarning($"变量名 '{key}' 不存在，返回默认值");
        return defaultValue;
    }

    /// <summary>
    /// 设置变量值
    /// </summary>
    public void Set(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogWarning("变量名不能为空");
            return;
        }

        var dict = GetRuntimeDict();

        if (dict.ContainsKey(key))
        {
            dict[key] = value;
        }
        else
        {
            dict.Add(key, value);
        }

        var existing = variableList.Find(v => v.Name == key);
        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            Variable<T> newVar = new Variable<T>();
            newVar.Name = key;
            newVar.Value = value;
            variableList.Add(newVar);
        }
    }

    /// <summary>
    /// 检查变量是否存在
    /// </summary>
    public bool Has(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;

        var dict = GetRuntimeDict();
        return dict.ContainsKey(key);
    }

    /// <summary>
    /// 获取原始列表
    /// </summary>
    public List<Variable<T>> GetList() => variableList;

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器专用：强制刷新字典
    /// </summary>
    public void RefreshInEditor()
    {
        BuildRuntimeDict();
    }
#endif
}