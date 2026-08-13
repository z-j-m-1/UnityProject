using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZGameFramework.Core;

[CreateNodeMenu("变换/移动")]
// 物体移动节点
// 物体移动节点
[NodeTint("#44AAFF")]
public class MoveObjectNode : FlowNode
{
    public Vector3 moveOffset; // 移动偏移量输入端口

    public override void Execute()
    {
        MoveObjectNodeEvent.Trigger(evt =>
        {
            evt.targetObject = (graph as BaseNodeGraph).attachedObject; // 获取节点图绑定的物体
            evt.moveOffset = moveOffset;
        });
    }
}

public class MoveObjectNodeEvent : ParameterizedEvent<MoveObjectNodeEvent>
{
    public GameObject targetObject; // 目标物体
    public Vector3 moveOffset; // 移动偏移量输入端口
    public override void OnRecycled()
    {
        targetObject = null;
        moveOffset = Vector3.zero;
    }
}


