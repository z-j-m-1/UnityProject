/// <summary>
/// UI 来源
/// </summary>
public enum UISource
{
    /// <summary>自身及子物体（图附加物体的层级）</summary>
    Self,

    /// <summary>Canvas 下任意 UI（通过 UI 收集者按名字查找）</summary>
    Canvas
}

/// <summary>
/// UI 组件类型
/// </summary>
public enum UIType
{
    Text,
    TextMeshPro,
    Image
}
