using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZGameFramework.Core;


[CreateNodeMenu("变换/旋转")]
// 物体移动节点
// 物体移动节点
[NodeTint("#44AAFF")]
[NodeWidth(300)]
public class RoteObjectNode : FlowNode
{
    public Vector3 roteOffset; // 移动偏移量输入端口

    public override void Execute()
    {
        RoteObjectNodeEvent.Trigger(evt =>
        {
            evt.targetObject = (graph as BaseNodeGraph).attachedObject; // 获取节点图绑定的物体
            evt.roteOffset = roteOffset;
        });
    }
}

public class RoteObjectNodeEvent : ParameterizedEvent<RoteObjectNodeEvent>
{
    public GameObject targetObject; // 目标物体
    public Vector3 roteOffset; // 移动偏移量输入端口
    public override void OnRecycled()
    {
        targetObject = null;
        roteOffset = Vector3.zero;
    }
}


