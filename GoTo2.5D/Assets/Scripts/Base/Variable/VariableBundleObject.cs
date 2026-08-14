using UnityEngine;

/// <summary>
/// 变量捆绑对象（ScriptableObject）- 持久变量容器资产，可在 Inspector 中手动编辑变量
/// 房间变量：按场景名放到 Resources/PersistentVariables/Room/ 下，运行时按场景加载
/// 全局变量：手动拖到 GameGlobalVariableManager 的 variableObject 字段上（调试方便）
/// </summary>
[CreateAssetMenu(menuName = "变量/变量捆绑对象", fileName = "VariableBundleObject")]
public class VariableBundleObject : ScriptableObject
{
    [SerializeField] private VariableBundle variables = new VariableBundle();

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

    /// <summary>
    /// 重建内部缓存字典
    /// </summary>
    public void Rebuild()
    {
        variables.Rebuild();
    }
}
