using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateNodeMenu("其他/打印")]
// 打印节点 - 继承流程节点
[NodeTint("#ff00b3")] // 橙色便于识别
public class PrintNode : FlowNode
{
    [Input(ShowBackingValue.Unconnected)]
    public string printMessage = "打印信息";

    public override void Execute()
    {
        Debug.Log(printMessage);
    }
}
