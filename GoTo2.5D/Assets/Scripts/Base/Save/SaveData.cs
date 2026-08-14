using System;
using System.Collections.Generic;

/// <summary>
/// 变量捆绑数据 - 存档用 LitJson 序列化的中转结构（全公共字段）
/// </summary>
[Serializable]
public class VariableBundleData
{
    public Dictionary<string, string> strings;
    public Dictionary<string, bool> bools;
    public Dictionary<string, int> ints;
    public Dictionary<string, float> floats;
}

/// <summary>
/// 存档数据 - 存储需要持久化的三类变量：节点图变量 / 房间变量（按场景分槽）/ 全局变量
/// </summary>
[Serializable]
public class SaveData
{
    /// <summary>存档版本号</summary>
    public int version = 1;

    /// <summary>存档时的场景名，读档后用于定位房间变量槽</summary>
    public string sceneName;

    /// <summary>节点图变量：图资产名 → 变量数据</summary>
    public Dictionary<string, VariableBundleData> graphs;

    /// <summary>房间变量：场景名 → 变量数据（按场景分槽）</summary>
    public Dictionary<string, VariableBundleData> roomByScene;

    /// <summary>全局变量</summary>
    public VariableBundleData global;
}
