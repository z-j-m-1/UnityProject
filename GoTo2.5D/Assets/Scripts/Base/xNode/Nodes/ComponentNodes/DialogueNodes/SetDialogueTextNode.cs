using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 对话UI-设置对话文本：把文本写进 DialogueBoxController，并按 startTypewriter 决定
/// 是否紧接着通知 TMPWriter 启动打字机（调用 DialogueBoxController.StartDialogue）。
///
/// 「等打字结束」有两种方式（**互斥**，见 waitUntilTyped 的说明）：
///   ① waitUntilTyped = true（默认）：本节点自己在 GetFlow 里等完，一个节点搞定，但没有超时保护；
///   ② waitUntilTyped = false：链立即继续，把「打字已结束」输出端口接到「流程/等待条件」，
///      由等待节点去等 —— 白拿 timeout 与 timeoutTo 超时分支（例：打字卡住就跳过）。
///
/// 目标物体由「目标物体」输入端口提供（接 取值/获取物体(自身|名称|引用)）。
///
/// ⚠️ shouldWait / dialogueBox 是节点实例状态：同一张图被多条链并发跑过本节点时会互相覆盖
///    （与 ForLoopNode.index、MoveToNode.targetTransform 等共享节点状态的性质相同）。
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

    [Header("等打字结束再走下一个节点")]
    [Tooltip("勾选（默认）：本节点自己在 GetFlow 里等 TMPWriter 把整段文字逐字显示完，链才继续下一个节点 —— " +
             "一个节点搞定，但拿不到超时保护。\n" +
             "取消：启动打字机后立即继续；此时把「打字已结束」输出端口接到「流程/等待条件」，" +
             "可让等待带 timeout 与 timeoutTo 超时分支。\n" +
             "★ 两种方式互斥：勾选时下游读到的「打字已结束」必然为 true（因为本节点已经等完了）。\n" +
             "仅在 startTypewriter 为真时生效。")]
    public bool waitUntilTyped = true;

    [Header("打字已结束（配合 流程/等待条件 可带超时）")]
    [Output]
    public bool typingFinished;

    // 本次执行是否要等（Apply 里确定）
    private bool shouldWait;
    private DialogueBoxController dialogueBox;

    /// <summary>每次执行先清掉上一次的状态，避免目标解析失败时沿用陈旧状态而错误等待</summary>
    public override void Execute()
    {
        shouldWait = false;
        dialogueBox = null;
        base.Execute();
    }

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

        dialogueBox = box;
        shouldWait = waitUntilTyped && start;

        NodeLog.Info($"{GetType().Name}: 对话文本已设置（{content?.Length ?? 0} 字），打字机{(start ? "已启动" : "未启动")}{(shouldWait ? "，等待打字结束" : "")}");
    }

    /// <summary>
    /// 等打字结束：TMPWriter 启动后 IsWriting 立即为 true（OnStartWriting 在协程第一个 yield 之前同步执行），
    /// 逐字显示完变 false —— 所以逐帧轮询即可，不存在"启动前就判定为结束"的时序竞态。
    /// </summary>
    public override IEnumerator GetFlow()
    {
        if (!shouldWait || dialogueBox == null) yield break;

        while (dialogueBox.IsTyping)
        {
            yield return null;
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(dialogueText))
            return GetInputValue<string>(nameof(dialogueText), dialogueText);
        if (port.fieldName == nameof(startTypewriter))
            return GetInputValue<bool>(nameof(startTypewriter), startTypewriter);

        if (port.fieldName == nameof(typingFinished))
        {
            // 每次读取都实时求值（下游「流程/等待条件」会逐帧读它）
            // dialogueBox == null（本节点还没执行过，或目标没解析到）→ 视为"已结束"：
            // fail-open，避免配置错误把剧情卡死（错配已在 Apply 里警告过）
            typingFinished = dialogueBox == null || !dialogueBox.IsTyping;
            return typingFinished;
        }
        return null;
    }
}
