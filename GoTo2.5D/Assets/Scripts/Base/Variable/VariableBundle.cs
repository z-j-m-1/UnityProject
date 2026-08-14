using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 常用变量捆绑集合 - 将 string / bool / int 三个变量容器捆成一组，统一提供泛型读写
/// </summary>
[Serializable]
public class VariableBundle
{
    [SerializeField] private VariableContainer<string> stringContainer = new VariableContainer<string>();
    [SerializeField] private VariableContainer<bool> boolContainer = new VariableContainer<bool>();
    [SerializeField] private VariableContainer<int> intContainer = new VariableContainer<int>();

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

    /// <summary>
    /// 重建内部缓存字典
    /// </summary>
    public void Rebuild()
    {
        stringContainer.Rebuild();
        boolContainer.Rebuild();
        intContainer.Rebuild();
    }

    /// <summary>
    /// 从存档数据导入运行值
    /// </summary>
    public void ImportFrom(VariableBundleData data)
    {
        if (data == null) return;
        stringContainer.ImportFrom(data.strings);
        boolContainer.ImportFrom(data.bools);
        intContainer.ImportFrom(data.ints);
    }

    /// <summary>
    /// 导出当前运行值到存档数据
    /// </summary>
    public VariableBundleData Export()
    {
        return new VariableBundleData
        {
            strings = stringContainer.Export(),
            bools = boolContainer.Export(),
            ints = intContainer.Export()
        };
    }
}
