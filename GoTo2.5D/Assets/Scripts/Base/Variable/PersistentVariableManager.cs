using UnityEngine;

/// <summary>
/// 持久变量管理器抽象基类（房间 / 游戏全局单例共用）
/// </summary>
public abstract class PersistentVariableManager : MonoBehaviour
{
    [SerializeField] protected VariableBundle variables = new VariableBundle();

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
        return variables.Get(key, defaultValue);
    }

    /// <summary>
    /// 设置变量值
    /// </summary>
    public void Set<T>(string key, T value)
    {
        variables.Set(key, value);
    }

    /// <summary>
    /// 检查变量是否存在
    /// </summary>
    public bool Has<T>(string key)
    {
        return variables.Has<T>(key);
    }

    // ============ 订阅持久变量事件（供节点使用） ============

    protected virtual void OnEnable()
    {
        PersistentSetVariableEvent<string>.Subscribe(OnSetVariable<string>);
        PersistentSetVariableEvent<bool>.Subscribe(OnSetVariable<bool>);
        PersistentSetVariableEvent<int>.Subscribe(OnSetVariable<int>);

        PersistentGetVariableEvent<string>.Subscribe(OnGetVariable<string>);
        PersistentGetVariableEvent<bool>.Subscribe(OnGetVariable<bool>);
        PersistentGetVariableEvent<int>.Subscribe(OnGetVariable<int>);
    }

    protected virtual void OnDisable()
    {
        PersistentSetVariableEvent<string>.Unsubscribe(OnSetVariable<string>);
        PersistentSetVariableEvent<bool>.Unsubscribe(OnSetVariable<bool>);
        PersistentSetVariableEvent<int>.Unsubscribe(OnSetVariable<int>);

        PersistentGetVariableEvent<string>.Unsubscribe(OnGetVariable<string>);
        PersistentGetVariableEvent<bool>.Unsubscribe(OnGetVariable<bool>);
        PersistentGetVariableEvent<int>.Unsubscribe(OnGetVariable<int>);
    }

    private void OnSetVariable<T>(PersistentSetVariableEvent<T> evt)
    {
        if (evt.scope != Scope) return;
        variables.Set(evt.variableName, evt.variableValue);
    }

    private void OnGetVariable<T>(PersistentGetVariableEvent<T> evt)
    {
        if (evt.scope != Scope) return;
        evt.callback?.Invoke(variables.Get(evt.variableName, evt.defaultValue));
    }
}
