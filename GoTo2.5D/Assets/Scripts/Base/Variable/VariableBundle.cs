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
    [SerializeField] private VariableContainer<float> floatContainer = new VariableContainer<float>();
    [SerializeField] private VariableContainer<Vector3> vector3Container = new VariableContainer<Vector3>();

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
        if (type == typeof(float))
        {
            return (T)(object)floatContainer.Get(key, (float)(object)defaultValue);
        }
        if (type == typeof(Vector3))
        {
            return (T)(object)vector3Container.Get(key, (Vector3)(object)defaultValue);
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
        if (type == typeof(float))
        {
            floatContainer.Set(key, (float)(object)value);
            return;
        }
        if (type == typeof(Vector3))
        {
            vector3Container.Set(key, (Vector3)(object)value);
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
        if (type == typeof(float))
        {
            return floatContainer.Has(key);
        }
        if (type == typeof(Vector3))
        {
            return vector3Container.Has(key);
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
        floatContainer.Rebuild();
        vector3Container.Rebuild();
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
        floatContainer.ImportFrom(data.floats);
        vector3Container.ImportFrom(data.vector3s);
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
            ints = intContainer.Export(),
            floats = floatContainer.Export(),
            vector3s = vector3Container.Export()
        };
    }

    /// <summary>
    /// 名字优先 + GUID 兜底解析（得到实际名字与 GUID）
    /// </summary>
    public bool TryResolve<T>(string name, string guid, out T value, out string actualName, out string actualGuid)
    {
        value = default;
        actualName = null;
        actualGuid = null;
        Type type = typeof(T);

        if (type == typeof(string))
        {
            if (stringContainer.TryResolve(name, guid, out string s, out actualName, out actualGuid))
            {
                value = (T)(object)s;
                return true;
            }
            return false;
        }
        if (type == typeof(bool))
        {
            if (boolContainer.TryResolve(name, guid, out bool b, out actualName, out actualGuid))
            {
                value = (T)(object)b;
                return true;
            }
            return false;
        }
        if (type == typeof(int))
        {
            if (intContainer.TryResolve(name, guid, out int i, out actualName, out actualGuid))
            {
                value = (T)(object)i;
                return true;
            }
            return false;
        }
        if (type == typeof(float))
        {
            if (floatContainer.TryResolve(name, guid, out float f, out actualName, out actualGuid))
            {
                value = (T)(object)f;
                return true;
            }
            return false;
        }
        if (type == typeof(Vector3))
        {
            if (vector3Container.TryResolve(name, guid, out Vector3 v, out actualName, out actualGuid))
            {
                value = (T)(object)v;
                return true;
            }
            return false;
        }
        return false;
    }

    /// <summary>
    /// 名字优先 + GUID 兜底解析后设置值（仅对已存在的变量生效；都不存在返回 false）
    /// </summary>
    public bool TryResolveAndSet<T>(string name, string guid, T value, out string actualName, out string actualGuid)
    {
        actualName = null;
        actualGuid = null;
        Type type = typeof(T);

        if (type == typeof(string)) return stringContainer.TryResolveAndSet(name, guid, value as string, out actualName, out actualGuid);
        if (type == typeof(bool)) return boolContainer.TryResolveAndSet(name, guid, (bool)(object)value, out actualName, out actualGuid);
        if (type == typeof(int)) return intContainer.TryResolveAndSet(name, guid, (int)(object)value, out actualName, out actualGuid);
        if (type == typeof(float)) return floatContainer.TryResolveAndSet(name, guid, (float)(object)value, out actualName, out actualGuid);
        if (type == typeof(Vector3)) return vector3Container.TryResolveAndSet(name, guid, (Vector3)(object)value, out actualName, out actualGuid);
        return false;
    }
}
