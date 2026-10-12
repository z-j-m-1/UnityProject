using UnityEngine;
using XNode;

/// <summary>
/// 对话UI-跳过打字：立刻把整段对话文字显示完（<c>TMPWriter.SkipWriter</c>），
/// 并按 <c>stopVoice</c> 决定是否同时停掉正在播的语音 —— 给"点击跳过"按钮用。
///
/// 与「对话UI/台词」的分工：台词负责**设置与启动**，本节点负责"玩家不想等"的那一下。
/// 「台词」节点里"本句没有语音就停掉上一句"是自动的，不需要本节点；本节点管的是**中途跳过**。
/// 目标物体由「目标物体」输入端口提供（接 取值/获取物体 系列节点）。
/// </summary>
[CreateNodeMenu("对话UI/跳过打字")]
[NodeTint("#FF99CC")]
public class SkipDialogueNode : ComponentActionNode<DialogueBoxController>
{
    [Header("同时停掉语音（默认开）")]
    public bool stopVoice = true;

    protected override void Apply(DialogueBoxController box)
    {
        box.SkipDialogue(stopVoice);
        NodeLog.Info($"{GetType().Name}: 已跳过打字{(stopVoice ? "，并停止语音" : "")}");
    }
}
