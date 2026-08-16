using System;
using UnityEngine;
using XNode;

/// <summary>
/// 子图参数输出-物体（返回值槽，不序列化）：子图内部把目标物体连到本节点输入端口，
/// 子图链跑完后父图求值读回。不走变量系统（GameObject 不入 VariableBundle 序列化）。
/// </summary>
[CreateNodeMenu("子图/参数输出/物体")]
public class SubGraphOutputGameObjectNode : SubGraphOutputNodeBase
{
    [Input(ShowBackingValue.Never, ConnectionType.Override)]
    [System.NonSerialized]
    public GameObject value;

    public override Type ParamType => typeof(GameObject);

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(value))
        {
            return EvaluateValue();
        }
        return null;
    }

    public override object EvaluateValue()
    {
        return GetInputValue<GameObject>(nameof(value), value);
    }
}