using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个变量条目存档数据（名字 + GUID + 值）
/// </summary>
[Serializable]
public class VariableEntryData<T>
{
    public string name;
    public string guid;
    public T value;
}

/// <summary>
/// 变量捆绑数据 - 存档用 LitJson 序列化的中转结构（全公共字段）
/// </summary>
[Serializable]
public class VariableBundleData
{
    public List<VariableEntryData<string>> strings;
    public List<VariableEntryData<bool>> bools;
    public List<VariableEntryData<int>> ints;
    public List<VariableEntryData<float>> floats;
    public List<VariableEntryData<Vector3>> vector3s;
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
    public string roomId;   // 房间稳定ID（RoomIdentity.roomId），用于场景改名后按ID匹配存档
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
