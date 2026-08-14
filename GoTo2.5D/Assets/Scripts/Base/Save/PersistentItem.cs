using UnityEngine;

/// <summary>
/// 持久化物品基类 - 挂在需要存档还原状态的物品/机关上
/// 物品状态直接存放在变量包 Variables 中，管理器直接导入导出；
/// 移除逻辑由物品自己处理（在 OnAfterLoad 里读自己的移除变量）
/// </summary>
public class PersistentItem : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private string itemId;
    [SerializeField] private VariableBundle variables = new VariableBundle();

    /// <summary>物品唯一标识（场景内唯一，稳定不变；无则自动生成）</summary>
    public string ItemId
    {
        get
        {
            EnsureGuid();
            return itemId;
        }
    }

    /// <summary>物品状态变量包（直接读写：Get/Set/Has）</summary>
    public VariableBundle Variables => variables;

    private void EnsureGuid()
    {
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = System.Guid.NewGuid().ToString();
        }
    }

    public void OnBeforeSerialize()
    {
        EnsureGuid();
    }

    public void OnAfterDeserialize()
    {
        EnsureGuid();
    }

    /// <summary>
    /// 重新生成 GUID（仅当你确实需要更换标识时使用；会破坏已有存档中的对应关系）
    /// </summary>
    [ContextMenu("重新生成GUID")]
    public void RegenerateGuid()
    {
        itemId = System.Guid.NewGuid().ToString();
        Debug.Log($"PersistentItem '{name}' 已重新生成 GUID: {itemId}");
    }

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
    /// 保存前钩子（可选）：把外部状态（如 Transform 位置）同步进 Variables
    /// </summary>
    public virtual void OnBeforeSave() { }

    /// <summary>
    /// 读档后钩子（可选）：根据 Variables 调整外部状态（如 removed 则隐藏）
    /// 示例：if (Variables.Get&lt;bool&gt;("removed", false)) gameObject.SetActive(false);
    /// </summary>
    public virtual void OnAfterLoad() { }
}

