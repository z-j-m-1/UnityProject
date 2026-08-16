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

    /// <summary>运行时：名字 → GUID</summary>
    [NonSerialized] private Dictionary<string, string> nameToGuid = new Dictionary<string, string>();

    /// <summary>运行时：GUID → 名字</summary>
    [NonSerialized] private Dictionary<string, string> guidToName = new Dictionary<string, string>();

    /// <summary>运行时：名字 → 是否持久化（来自列表 persist 标志；运行时新建默认持久化）</summary>
    [NonSerialized] private Dictionary<string, bool> nameToPersist = new Dictionary<string, bool>();

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
    /// 从列表构建字典（同时构建名字 ↔ GUID 映射）
    /// </summary>
    private void BuildRuntimeDict()
    {
        runtimeDict = new Dictionary<string, T>();
        nameToGuid.Clear();
        guidToName.Clear();
        nameToPersist.Clear();

        foreach (var variable in variableList)
        {
            if (variable != null && !string.IsNullOrEmpty(variable.Name))
            {
                if (!runtimeDict.ContainsKey(variable.Name))
                {
                    runtimeDict.Add(variable.Name, variable.Value);
                    string guid = variable.Guid;
                    nameToGuid[variable.Name] = guid;
                    guidToName[guid] = variable.Name;
                    nameToPersist[variable.Name] = variable.persist;
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

        // 保证名字→GUID 映射存在（运行时新建变量生成 GUID）
        if (!nameToGuid.ContainsKey(key))
        {
            string guid = System.Guid.NewGuid().ToString();
            nameToGuid[key] = guid;
            guidToName[guid] = key;
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
    /// 从外部数据导入运行值（存档加载时调用，替换运行字典，含 GUID）
    /// </summary>
    public void ImportFrom(List<VariableEntryData<T>> entries)
    {
        // 合并式：先以列表（设计默认值）构建运行时字典，再用存档条目覆盖持久化变量
        // - 非持久化变量保持列表默认值（每次开始游戏重置）
        // - 存档里没有的列表新变量也以默认值出现（修掉旧的"存档完全替换字典"隐患）
        BuildRuntimeDict();

        if (entries == null) return;
        foreach (var e in entries)
        {
            if (e == null || string.IsNullOrEmpty(e.name)) continue;

            // 非持久化变量：跳过存档覆盖，保持列表默认值
            if (nameToPersist.TryGetValue(e.name, out bool persist) && !persist) continue;

            string guid = string.IsNullOrEmpty(e.guid) ? System.Guid.NewGuid().ToString() : e.guid;
            runtimeDict[e.name] = e.value;
            nameToGuid[e.name] = guid;
            guidToName[guid] = e.name;
            if (!nameToPersist.ContainsKey(e.name))
            {
                nameToPersist[e.name] = true;   // 存档中有但列表没有的名字：视为持久化
            }
        }
    }

    /// <summary>
    /// 导出当前运行值（存档保存时调用，含 GUID）
    /// </summary>
    public List<VariableEntryData<T>> Export()
    {
        var dict = GetRuntimeDict();
        var result = new List<VariableEntryData<T>>(dict.Count);
        foreach (var kvp in dict)
        {
            // 非持久化变量不进存档
            if (nameToPersist.TryGetValue(kvp.Key, out bool persist) && !persist) continue;

            nameToGuid.TryGetValue(kvp.Key, out string guid);
            result.Add(new VariableEntryData<T> { name = kvp.Key, guid = guid, value = kvp.Value });
        }
        return result;
    }

    /// <summary>
    /// 名字优先 + GUID 兜底解析（得到实际名字与 GUID）
    /// </summary>
    public bool TryResolve(string name, string guid, out T value, out string actualName, out string actualGuid)
    {
        value = default;
        actualName = null;
        actualGuid = null;
        var dict = GetRuntimeDict();

        // 1) 名字优先
        if (!string.IsNullOrEmpty(name) && dict.TryGetValue(name, out value))
        {
            actualName = name;
            actualGuid = nameToGuid.TryGetValue(name, out string g) ? g : null;
            return true;
        }

        // 2) GUID 兜底
        if (!string.IsNullOrEmpty(guid) && guidToName.TryGetValue(guid, out string resolvedName))
        {
            if (dict.TryGetValue(resolvedName, out value))
            {
                actualName = resolvedName;
                actualGuid = guid;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 名字优先 + GUID 兜底解析后设置值（仅对已存在的变量生效；都不存在返回 false）
    /// </summary>
    public bool TryResolveAndSet(string name, string guid, T value, out string actualName, out string actualGuid)
    {
        actualName = null;
        actualGuid = null;
        var dict = GetRuntimeDict();

        string targetName = null;
        if (!string.IsNullOrEmpty(name) && dict.ContainsKey(name))
        {
            targetName = name;
        }
        else if (!string.IsNullOrEmpty(guid) && guidToName.TryGetValue(guid, out string resolvedName) && dict.ContainsKey(resolvedName))
        {
            targetName = resolvedName;
        }

        if (targetName == null) return false;

        dict[targetName] = value;
        actualName = targetName;
        actualGuid = nameToGuid.TryGetValue(targetName, out string g) ? g : null;
        return true;
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