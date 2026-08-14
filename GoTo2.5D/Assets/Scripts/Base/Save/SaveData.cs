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
/// 房间存档数据 - 每个房间一个文件（staging 预备 / archive 存档 各一份）
/// 包含该房间的局部变量、该房间各节点图变量（按图GUID）、物品状态（后续扩展）
/// </summary>
[Serializable]
public class RoomSaveData
{
    public int version = 1;
    public string sceneName;
    public VariableBundleData localVariables;
    public Dictionary<string, VariableBundleData> graphs;   // 图GUID → 图变量
    public Dictionary<string, VariableBundleData> items;    // itemId → 物品状态（预留）
}

/// <summary>
/// 全局存档数据 - 全局变量（不属于任何房间）
/// </summary>
[Serializable]
public class GlobalSaveData
{
    public int version = 1;
    public VariableBundleData global;
}

/// <summary>
/// 存档索引/元数据 - 记录存档场景与时间
/// </summary>
[Serializable]
public class SaveIndex
{
    public int version = 1;
    public string lastScene;
    public string saveTime;
}
