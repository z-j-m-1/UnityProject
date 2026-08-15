/// <summary>
/// 变量操作的目标来源
/// </summary>
public enum VariableSource
{
    /// <summary>本图变量</summary>
    Self,

    /// <summary>跨图通讯（按目标图/物体名）</summary>
    ExternalGraph,

    /// <summary>房间持久变量</summary>
    Room,

    /// <summary>全局持久变量</summary>
    Global
}
