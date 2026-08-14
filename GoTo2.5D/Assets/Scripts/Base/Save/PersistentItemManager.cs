using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 持久化物品管理器（单例）- 收集/分发每个房间的物品状态
/// </summary>
public class PersistentItemManager : MonoBehaviour
{
    private static PersistentItemManager _instance;

    /// <summary>当前场景已注册的活跃物品</summary>
    private readonly List<PersistentItem> activeItems = new List<PersistentItem>();

    /// <summary>已禁用/销毁物品的待存状态（itemId → 数据）</summary>
    private readonly Dictionary<string, VariableBundleData> pendingData = new Dictionary<string, VariableBundleData>();

    /// <summary>是否已初始化（供存档系统判断是否需要采集物品）</summary>
    public static bool IsInitialized => _instance != null;

    /// <summary>
    /// 物品管理器单例（不存在则自动创建）
    /// </summary>
    public static PersistentItemManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PersistentItemManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(PersistentItemManager).Name);
                    _instance = go.AddComponent<PersistentItemManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>注册物品（OnEnable 时调用）</summary>
    public void Register(PersistentItem item)
    {
        if (item == null || activeItems.Contains(item)) return;
        if (string.IsNullOrEmpty(item.ItemId))
        {
            Debug.LogWarning($"PersistentItem 缺少 itemId：{item.name}", item);
        }
        activeItems.Add(item);
    }

    /// <summary>注销物品（OnDisable 时调用）</summary>
    public void Unregister(PersistentItem item)
    {
        if (item != null) activeItems.Remove(item);
    }

    /// <summary>清除某物品的待存状态（重新启用时调用，避免重复）</summary>
    public void ClearPending(string itemId)
    {
        if (!string.IsNullOrEmpty(itemId)) pendingData.Remove(itemId);
    }

    /// <summary>采集某物品的当前状态到待存缓存（OnDisable 时调用）</summary>
    public void CaptureItem(PersistentItem item)
    {
        if (item == null) return;
        item.OnBeforeSave();
        pendingData[item.ItemId] = item.Variables.Export();
    }

    /// <summary>导出当前房间的所有物品状态（活跃物品 + 待存缓存），并清空待存缓存</summary>
    public Dictionary<string, VariableBundleData> ExportCurrent()
    {
        Dictionary<string, VariableBundleData> result = new Dictionary<string, VariableBundleData>();

        foreach (PersistentItem item in activeItems)
        {
            if (item == null) continue;
            item.OnBeforeSave();
            result[item.ItemId] = item.Variables.Export();
        }

        foreach (KeyValuePair<string, VariableBundleData> kvp in pendingData)
        {
            result[kvp.Key] = kvp.Value;
        }
        pendingData.Clear();

        return result;
    }

    /// <summary>把存档的物品状态分发给当前场景的物品（进入房间/读档时调用）</summary>
    public void ApplyCurrent(Dictionary<string, VariableBundleData> itemData)
    {
        if (itemData == null) return;
        foreach (PersistentItem item in activeItems)
        {
            if (item == null) continue;
            if (itemData.TryGetValue(item.ItemId, out VariableBundleData data))
            {
                item.Variables.ImportFrom(data);
                item.OnAfterLoad();
            }
        }
    }
}
