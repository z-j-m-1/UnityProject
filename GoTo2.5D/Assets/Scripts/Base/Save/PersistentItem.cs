using UnityEngine;

/// <summary>
/// 持久化物品基类 - 挂在需要存档还原状态的物品/机关上
/// 物品自己决定保存什么、如何还原（移除逻辑也由物品自己处理）
/// </summary>
public class PersistentItem : MonoBehaviour
{
    [SerializeField] private string itemId;

    /// <summary>物品唯一标识（场景内唯一，稳定不变）</summary>
    public string ItemId => itemId;

    protected virtual void OnEnable()
    {
        // 注册进管理器（不存在会自动创建），并清除旧的待存状态
        PersistentItemManager.Instance.Register(this);
        PersistentItemManager.Instance.ClearPending(itemId);
    }

    protected virtual void OnDisable()
    {
        // 物品被禁用/销毁（含切场景）时，把当前状态采集进待存缓存
        if (!PersistentItemManager.IsInitialized) return;
        PersistentItemManager.Instance.Unregister(this);
        PersistentItemManager.Instance.CaptureItem(this);
    }

    /// <summary>
    /// 物品把当前状态写入存档数据（离开房间/保存时调用）
    /// </summary>
    public virtual void OnSaveState(VariableBundleData data) { }

    /// <summary>
    /// 物品根据存档数据还原自己（进入房间/读档时调用）
    /// 示例：if (data.bools != null && data.bools.TryGetValue("removed", out bool removed) && removed) gameObject.SetActive(false);
    /// </summary>
    public virtual void OnLoadState(VariableBundleData data) { }
}
