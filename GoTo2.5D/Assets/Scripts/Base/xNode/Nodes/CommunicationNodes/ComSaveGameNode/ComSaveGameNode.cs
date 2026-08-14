using UnityEngine;
using XNode;

/// <summary>
/// 通讯-保存游戏节点（存档点）- 执行到该节点时自动保存当前状态
/// </summary>
[CreateNodeMenu("通讯/存档/保存游戏")]
public class ComSaveGameNode : FlowNode
{
    public override void Execute()
    {
        SaveSystem.Save();
        Debug.Log($"{GetType().Name}: 触发保存存档");

        // 执行下一个节点
        base.Execute();
    }

    public override object GetValue(NodePort port)
    {
        return null;
    }
}
