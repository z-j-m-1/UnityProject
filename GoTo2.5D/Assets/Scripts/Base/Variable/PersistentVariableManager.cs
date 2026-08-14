using UnityEngine;

/// <summary>
/// 持久变量管理器抽象基类（房间 / 游戏全局单例共用）
/// </summary>
public abstract class PersistentVariableManager : MonoBehaviour
{
    /// <summary>
    /// 当前使用的变量对象
    /// 房间：由 RoomVariableManager 按场景从 Resources 加载
    /// 全局：可在 Inspector 手动拖入（调试用），未拖入时自动创建运行时实例
    /// </summary>
    [SerializeField] private VariableBundleObject variableObject;

    /// <summary>
    /// 获取当前变量对象，为空时自动创建运行时实例兜底
    /// </summary>
    protected VariableBundleObject VariableObject
    {
        get
        {
            if (variableObject == null)
            {
                variableObject = CreateVariableObject();
            }
            return variableObject;
        }
    }

    /// <summary>
    /// 当前管理器对应的作用域
    /// </summary>
    public abstract PersistentVariableScope Scope { get; }

    /// <summary>
    /// 根据作用域获取对应的管理器（不存在则自动创建）
    /// </summary>
    public static PersistentVariableManager GetManager(PersistentVariableScope scope)
    {
        switch (scope)
        {
            case PersistentVariableScope.Room:
                return RoomVariableManager.Instance;
            case PersistentVariableScope.Global:
                return GameGlobalVariableManager.Instance;
            default:
                return null;
        }
    }

    // ============ 非节点脚本直接调用 ============

    /// <summary>
    /// 获取变量值
    /// </summary>
    public T Get<T>(string key, T defaultValue = default)
    {
        return VariableObject.Get(key, defaultValue);
    }

    /// <summary>
    /// 设置变量值
    /// </summary>
    public void Set<T>(string key, T value)
    {
        VariableObject.Set(key, value);
    }

    /// <summary>
    /// 检查变量是否存在
    /// </summary>
    public bool Has<T>(string key)
    {
        return VariableObject.Has<T>(key);
    }

    /// <summary>
    /// 从存档数据导入变量（存档加载时调用）
    /// </summary>
    public void Import(VariableBundleData data)
    {
        VariableObject.ImportFrom(data);
    }

    /// <summary>
    /// 导出当前变量到存档数据（存档保存时调用）
    /// </summary>
    public VariableBundleData Export()
    {
        return VariableObject.Export();
    }

    /// <summary>
    /// 切换当前使用的变量对象（房间按场景切换时调用）
    /// </summary>
    protected void SetVariableObject(VariableBundleObject obj)
    {
        variableObject = obj;
    }

    /// <summary>
    /// 创建默认的运行时变量对象（子类可重写）
    /// </summary>
    protected virtual VariableBundleObject CreateVariableObject()
    {
        return ScriptableObject.CreateInstance<VariableBundleObject>();
    }

    /// <summary>
    /// 是否已手动指定了变量对象（Inspector 拖入）
    /// </summary>
    protected bool HasAssignedVariableObject => variableObject != null;

    // ============ 订阅持久变量事件（供节点使用） ============

    protected virtual void OnEnable()
    {
        PersistentSetVariableEvent<string>.Subscribe(OnSetVariable<string>);
        PersistentSetVariableEvent<bool>.Subscribe(OnSetVariable<bool>);
        PersistentSetVariableEvent<int>.Subscribe(OnSetVariable<int>);
        PersistentSetVariableEvent<float>.Subscribe(OnSetVariable<float>);

        PersistentGetVariableEvent<string>.Subscribe(OnGetVariable<string>);
        PersistentGetVariableEvent<bool>.Subscribe(OnGetVariable<bool>);
        PersistentGetVariableEvent<int>.Subscribe(OnGetVariable<int>);
        PersistentGetVariableEvent<float>.Subscribe(OnGetVariable<float>);
    }

    protected virtual void OnDisable()
    {
        PersistentSetVariableEvent<string>.Unsubscribe(OnSetVariable<string>);
        PersistentSetVariableEvent<bool>.Unsubscribe(OnSetVariable<bool>);
        PersistentSetVariableEvent<int>.Unsubscribe(OnSetVariable<int>);
        PersistentSetVariableEvent<float>.Unsubscribe(OnSetVariable<float>);

        PersistentGetVariableEvent<string>.Unsubscribe(OnGetVariable<string>);
        PersistentGetVariableEvent<bool>.Unsubscribe(OnGetVariable<bool>);
        PersistentGetVariableEvent<int>.Unsubscribe(OnGetVariable<int>);
        PersistentGetVariableEvent<float>.Unsubscribe(OnGetVariable<float>);
    }

    private void OnSetVariable<T>(PersistentSetVariableEvent<T> evt)
    {
        if (evt.scope != Scope) return;
        VariableObject.Set(evt.variableName, evt.variableValue);
    }

    private void OnGetVariable<T>(PersistentGetVariableEvent<T> evt)
    {
        if (evt.scope != Scope) return;
        evt.callback?.Invoke(VariableObject.Get(evt.variableName, evt.defaultValue));
    }
}
