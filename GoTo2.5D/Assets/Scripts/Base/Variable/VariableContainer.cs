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

    /// <summary>
    /// 获取运行字典
    /// 运行时：懒加载并缓存（存档导入与运行时修改都保存在 dict 中）
    /// 编辑模式：每次从列表重建，保证读到列表（设计默认值）的最新数据
    /// </summary>
    private Dictionary<string, T> GetRuntimeDict()
    {
        if (Application.isPlaying)
        {
            if (runtimeDict == null)
            {
                BuildRuntimeDict();
            }
            return runtimeDict;
        }

        BuildRuntimeDict();
        return runtimeDict;
    }

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

        // 编辑模式（非运行）下同步写列表，作为设计默认值；运行时只写运行字典
        if (!Application.isPlaying)
        {
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
    }

    /// <summary>
    /// 从外部数据导入运行值（存档加载时调用，替换运行字典）
    /// </summary>
    public void ImportFrom(Dictionary<string, T> values)
    {
        runtimeDict = values == null ? new Dictionary<string, T>() : new Dictionary<string, T>(values);
    }

    /// <summary>
    /// 导出当前运行值（存档保存时调用）
    /// </summary>
    public Dictionary<string, T> Export()
    {
        return new Dictionary<string, T>(GetRuntimeDict());
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