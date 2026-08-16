using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 流程-等待节点：暂停执行链指定秒数（协程方式，不阻塞进程）
/// </summary>
[CreateNodeMenu("流程/等待")]
public class WaitNode : FlowNode
{
    [Header("等待秒数")]
    public float duration = 1f;

    public override void Execute()
    {
        NodeLog.Verbose($"WaitNode: 等待 {duration} 秒");
    }

    public override IEnumerator GetFlow()
    {
        yield return new WaitForSeconds(Mathf.Max(duration, 0.01f));
    }
}
