using System;
using UnityEngine;
using XNode;

/// <summary>
/// 子图参数输入-物体（取值源，不序列化）：父图 SubGraphNode 执行前把连线值写入本节点 value 字段，
/// 子图内部连本节点输出端口取目标物体。不走变量系统（GameObject 不入 VariableBundle 序列化）。
/// </summary>
[CreateNodeMenu("子图/参数输入/物体")]
public class SubGraphInputGameObjectNode : SubGraphInputNodeBase
{
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject value;

    public override Type ParamType => typeof(GameObject);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
        {
            return value;
        }
        return null;
    }
}