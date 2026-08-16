using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>外部参数值类型（面板可视化编辑用）</summary>
public enum GraphParamType
{
    String,
    Bool,
    Int,
    Float,
    Vector2,
    Vector3,
    GameObject
}

/// <summary>
/// 外部参数值（可序列化的类型联合）：只使用与 GraphParamEntry.type 对应的字段，其余在面板隐藏。
/// </summary>
[Serializable]
public class GraphParamValue
{
    [HideInInspector] public string stringValue;
    [HideInInspector] public bool boolValue;
    [HideInInspector] public int intValue;
    [HideInInspector] public float floatValue;
    [HideInInspector] public Vector2 vector2Value;
    [HideInInspector] public Vector3 vector3Value;
    [HideInInspector] public GameObject objectValue;
}

/// <summary>命名参数项：名称 + 类型 + 值（外部脚本 Inspector 可编辑）</summary>
[Serializable]
public class GraphParamEntry
{
    [Tooltip("参数名（与图内「参数/输入」节点的 paramName 一致）")]
    public string name;

    [Tooltip("参数类型（切换后值字段随之变化）")]
    public GraphParamType type = GraphParamType.Float;

    public GraphParamValue value = new GraphParamValue();

    /// <summary>按类型取运行期值</summary>
    public object GetValue()
    {
        switch (type)
        {
            case GraphParamType.String: return value.stringValue;
            case GraphParamType.Bool: return value.boolValue;
            case GraphParamType.Int: return value.intValue;
            case GraphParamType.Float: return value.floatValue;
            case GraphParamType.Vector2: return value.vector2Value;
            case GraphParamType.Vector3: return value.vector3Value;
            case GraphParamType.GameObject: return value.objectValue;
            default: return null;
        }
    }
}

/// <summary>
/// 外部参数包列表（Inspector 可视化编辑）：
/// 外部 MonoBehaviour 直接声明 public GraphParamList xxx;，面板里即可增删/改参数；
/// 运行时调用 Build() 得到 GraphParams，传给 GraphExecutor.ExecuteFromEntry / GraphEvent.data。
/// </summary>
[Serializable]
public class GraphParamList
{
    public List<GraphParamEntry> entries = new List<GraphParamEntry>();

    /// <summary>构建运行期参数包（空名跳过；重名警告并覆盖）</summary>
    public GraphParams Build()
    {
        GraphParams p = new GraphParams();
        if (entries == null) return p;
        HashSet<string> seen = new HashSet<string>();
        foreach (GraphParamEntry e in entries)
        {
            if (e == null || string.IsNullOrEmpty(e.name)) continue;
            if (!seen.Add(e.name))
            {
                NodeLog.Warning($"GraphParamList: 参数名 '{e.name}' 重复，后者覆盖前者");
            }
            p.Set(e.name, e.GetValue());
        }
        return p;
    }
}