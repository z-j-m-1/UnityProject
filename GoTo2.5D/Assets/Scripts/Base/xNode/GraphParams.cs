using System.Collections.Generic;

/// <summary>
/// 外部参数包：C# 代码在触发节点图时携带的命名参数（瞬态，不序列化、不进存档）。
/// 用法：
///   GraphParams p = new GraphParams();
///   p.Set("move", new Vector2(0, 1f));        // 输入轴
///   p.Set("jump", true);                       // 按键
///   executor.ExecuteFromEntry("OnInput", p);   // 或 GraphEvent.Trigger(e => { e.eventId = "OnInput"; e.data = p; });
/// 图内用「参数/输入/xxx」节点按 paramName 读取。
/// </summary>
public class GraphParams
{
    private readonly Dictionary<string, object> values = new Dictionary<string, object>();

    /// <summary>设置参数（同名覆盖）</summary>
    public void Set<T>(string name, T value)
    {
        if (string.IsNullOrEmpty(name)) return;
        values[name] = value;
    }

    /// <summary>读取参数；不存在或类型不匹配返回 fallback</summary>
    public T Get<T>(string name, T fallback = default)
    {
        if (!string.IsNullOrEmpty(name) && values.TryGetValue(name, out object value) && value is T typed)
        {
            return typed;
        }
        return fallback;
    }

    /// <summary>是否包含指定参数名</summary>
    public bool Contains(string name) => !string.IsNullOrEmpty(name) && values.ContainsKey(name);

    /// <summary>移除指定参数</summary>
    public bool Remove(string name) => !string.IsNullOrEmpty(name) && values.Remove(name);

    /// <summary>清空全部参数</summary>
    public void Clear() => values.Clear();

    /// <summary>参数名集合（注入/清理用）</summary>
    public ICollection<string> Keys => values.Keys;

    /// <summary>参数数据（遍历用）</summary>
    public IEnumerable<KeyValuePair<string, object>> Data => values;

    /// <summary>参数个数</summary>
    public int Count => values.Count;
}