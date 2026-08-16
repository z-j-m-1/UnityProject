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
    [SerializeField] private VariableContainer<Vector2> vector2Container = new VariableContainer<Vector2>();
    [SerializeField] private VariableContainer<List<string>> stringListContainer = new VariableContainer<List<string>>();
    [SerializeField] private VariableContainer<List<int>> intListContainer = new VariableContainer<List<int>>();
    [SerializeField] private VariableContainer<List<float>> floatListContainer = new VariableContainer<List<float>>();
    [SerializeField] private VariableContainer<List<Vector2>> vector2ListContainer = new VariableContainer<List<Vector2>>();
    [SerializeField] private VariableContainer<List<Vector3>> vector3ListContainer = new VariableContainer<List<Vector3>>();

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
        if (type == typeof(Vector2))
        {
            return (T)(object)vector2Container.Get(key, (Vector2)(object)defaultValue);
        }
        if (type == typeof(List<string>))
        {
            return (T)(object)stringListContainer.Get(key, defaultValue as List<string>);
        }
        if (type == typeof(List<int>))
        {
            return (T)(object)intListContainer.Get(key, defaultValue as List<int>);
        }
        if (type == typeof(List<float>))
        {
            return (T)(object)floatListContainer.Get(key, defaultValue as List<float>);
        }
        if (type == typeof(List<Vector2>))
        {
            return (T)(object)vector2ListContainer.Get(key, defaultValue as List<Vector2>);
        }
        if (type == typeof(List<Vector3>))
        {
            return (T)(object)vector3ListContainer.Get(key, defaultValue as List<Vector3>);
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
        if (type == typeof(Vector2))
        {
            vector2Container.Set(key, (Vector2)(object)value);
            return;
        }
        if (type == typeof(List<string>))
        {
            stringListContainer.Set(key, value as List<string>);
            return;
        }
        if (type == typeof(List<int>))
        {
            intListContainer.Set(key, value as List<int>);
            return;
        }
        if (type == typeof(List<float>))
        {
            floatListContainer.Set(key, value as List<float>);
            return;
        }
        if (type == typeof(List<Vector2>))
        {
            vector2ListContainer.Set(key, value as List<Vector2>);
            return;
        }
        if (type == typeof(List<Vector3>))
        {
            vector3ListContainer.Set(key, value as List<Vector3>);
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
        if (type == typeof(Vector2))
        {
            return vector2Container.Has(key);
        }
        if (type == typeof(List<string>))
        {
            return stringListContainer.Has(key);
        }
        if (type == typeof(List<int>))
        {
            return intListContainer.Has(key);
        }
        if (type == typeof(List<float>))
        {
            return floatListContainer.Has(key);
        }
        if (type == typeof(List<Vector2>))
        {
            return vector2ListContainer.Has(key);
        }
        if (type == typeof(List<Vector3>))
        {
            return vector3ListContainer.Has(key);
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
        vector2Container.Rebuild();
        stringListContainer.Rebuild();
        intListContainer.Rebuild();
        floatListContainer.Rebuild();
        vector2ListContainer.Rebuild();
        vector3ListContainer.Rebuild();
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
        vector2Container.ImportFrom(data.vector2s);
        stringListContainer.ImportFrom(data.stringLists);
        intListContainer.ImportFrom(data.intLists);
        floatListContainer.ImportFrom(data.floatLists);
        vector2ListContainer.ImportFrom(data.vector2Lists);
        vector3ListContainer.ImportFrom(data.vector3Lists);
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
            vector3s = vector3Container.Export(),
            vector2s = vector2Container.Export(),
            stringLists = stringListContainer.Export(),
            intLists = intListContainer.Export(),
            floatLists = floatListContainer.Export(),
            vector2Lists = vector2ListContainer.Export(),
            vector3Lists = vector3ListContainer.Export()
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
        if (type == typeof(Vector2))
        {
            if (vector2Container.TryResolve(name, guid, out Vector2 v2, out actualName, out actualGuid))
            {
                value = (T)(object)v2;
                return true;
            }
            return false;
        }
        if (type == typeof(List<string>))
        {
            if (stringListContainer.TryResolve(name, guid, out List<string> sl, out actualName, out actualGuid))
            {
                value = (T)(object)sl;
                return true;
            }
            return false;
        }
        if (type == typeof(List<int>))
        {
            if (intListContainer.TryResolve(name, guid, out List<int> il, out actualName, out actualGuid))
            {
                value = (T)(object)il;
                return true;
            }
            return false;
        }
        if (type == typeof(List<float>))
        {
            if (floatListContainer.TryResolve(name, guid, out List<float> fl, out actualName, out actualGuid))
            {
                value = (T)(object)fl;
                return true;
            }
            return false;
        }
        if (type == typeof(List<Vector2>))
        {
            if (vector2ListContainer.TryResolve(name, guid, out List<Vector2> v2l, out actualName, out actualGuid))
            {
                value = (T)(object)v2l;
                return true;
            }
            return false;
        }
        if (type == typeof(List<Vector3>))
        {
            if (vector3ListContainer.TryResolve(name, guid, out List<Vector3> v3l, out actualName, out actualGuid))
            {
                value = (T)(object)v3l;
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
        if (type == typeof(Vector2)) return vector2Container.TryResolveAndSet(name, guid, (Vector2)(object)value, out actualName, out actualGuid);
        if (type == typeof(List<string>)) return stringListContainer.TryResolveAndSet(name, guid, value as List<string>, out actualName, out actualGuid);
        if (type == typeof(List<int>)) return intListContainer.TryResolveAndSet(name, guid, value as List<int>, out actualName, out actualGuid);
        if (type == typeof(List<float>)) return floatListContainer.TryResolveAndSet(name, guid, value as List<float>, out actualName, out actualGuid);
        if (type == typeof(List<Vector2>)) return vector2ListContainer.TryResolveAndSet(name, guid, value as List<Vector2>, out actualName, out actualGuid);
        if (type == typeof(List<Vector3>)) return vector3ListContainer.TryResolveAndSet(name, guid, value as List<Vector3>, out actualName, out actualGuid);
        return false;
    }

    /// <summary>编辑器/调试用：收集图中已定义的全部变量名（跨类型，去重，按容器顺序）</summary>
    public List<string> GetAllVariableNames()
    {
        var names = new List<string>();
        AddNames(stringContainer.GetList(), names);
        AddNames(boolContainer.GetList(), names);
        AddNames(intContainer.GetList(), names);
        AddNames(floatContainer.GetList(), names);
        AddNames(vector3Container.GetList(), names);
        AddNames(vector2Container.GetList(), names);
        AddNames(stringListContainer.GetList(), names);
        AddNames(intListContainer.GetList(), names);
        AddNames(floatListContainer.GetList(), names);
        AddNames(vector2ListContainer.GetList(), names);
        AddNames(vector3ListContainer.GetList(), names);
        return names;
    }

    private static void AddNames<T>(List<Variable<T>> vars, List<string> names)
    {
        foreach (Variable<T> v in vars)
        {
            if (v != null && !string.IsNullOrEmpty(v.Name) && !names.Contains(v.Name))
            {
                names.Add(v.Name);
            }
        }
    }
}
