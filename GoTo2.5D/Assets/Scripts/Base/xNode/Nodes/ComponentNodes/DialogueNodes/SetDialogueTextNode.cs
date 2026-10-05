using UnityEngine;
using XNode;

/// <summary>
/// 对话UI-设置对话文本：把文本写进 DialogueBoxController，并按 startTypewriter 决定
/// 是否紧接着通知 TMPWriter 启动打字机（调用 DialogueBoxController.StartDialogue）。
/// 目标解析沿用 ComponentActionNode（GameObject 输入端口 &gt; Attached/ByName/Direct）。
/// </summary>
[CreateNodeMenu("对话UI/设置对话文本")]
[NodeTint("#FF99CC")]
public class SetDialogueTextNode : ComponentActionNode<DialogueBoxController>
{
    [Header("对话文本（可接 字符串 节点）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string dialogueText;

    [Header("修改后启动打字机")]
    [Tooltip("勾选（默认）：写完文本后调用 StartDialogue() 让 TMPWriter 开始逐字显示。\n" +
             "取消：只改文本，不启动打字机（由其它节点/按钮稍后触发）。")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public bool startTypewriter = true;

    protected override void Apply(DialogueBoxController box)
    {
        string content = GetInputValue<string>(nameof(dialogueText), dialogueText);
        box.DialogueText = content;

        // 读回校验：写入非空却读不到 → 目标上的 TextMeshProUGUI 引用未配置，赋值被静默忽略
        if (content != null && box.DialogueText == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{box.name}' 的对话文本引用未配置，设置被忽略");
            return;
        }

        bool start = GetInputValue<bool>(nameof(startTypewriter), startTypewriter);
        if (start)
        {
            box.StartDialogue();
        }

        NodeLog.Info($"{GetType().Name}: 对话文本已设置（{content?.Length ?? 0} 字），打字机{(start ? "已启动" : "未启动")}");
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(dialogueText))
            return GetInputValue<string>(nameof(dialogueText), dialogueText);
        if (port.fieldName == nameof(startTypewriter))
            return GetInputValue<bool>(nameof(startTypewriter), startTypewriter);
        return null;
    }
}
