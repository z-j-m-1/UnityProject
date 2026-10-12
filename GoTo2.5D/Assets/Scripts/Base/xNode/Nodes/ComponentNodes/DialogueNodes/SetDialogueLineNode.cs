using System.Collections;
using UnityEngine;
using XNode;

/// <summary>本句**没有**语音时，对上一句仍在播放的语音怎么处理</summary>
public enum PreviousVoiceMode
{
    /// <summary>让它播完（默认）—— 语音比文字长是常态，不该因为文字显示完就掐掉</summary>
    LetFinish,
    /// <summary>立刻停掉</summary>
    Stop
}

/// <summary>
/// 对话UI-台词：**一句完整台词** = 文本 + 打字机逐字音效 + 整句语音 + 打字机启动/等待。
///
/// 由原「对话UI/设置对话文本」扩展而来 —— 现在它还负责两个音频，所以改了名字与菜单（<c>对话UI/台词</c>）。
/// 改名时**保留了脚本 GUID**，所以已有图里的旧节点实例不受影响，并会自动获得新增字段。
///
/// 「等打字结束」有两种方式（**互斥**，见 waitUntilTyped 的说明）：
///   ① waitUntilTyped = true（默认）：本节点自己在 GetFlow 里等完，一个节点搞定，但没有超时保护；
///   ② waitUntilTyped = false：链立即继续，把「打字已结束」输出端口接到「流程/等待条件」，
///      由等待节点去等 —— 白拿 timeout 与 timeoutTo 超时分支（例：打字卡住就跳过）。
///
/// **两个音频的分工**（都可以留空 —— 留空就不发声，也不报错）：
///   · dialogueAudioClip：打字机**逐字**播放的短音效（留空 = 关闭逐字音效）；
///   · voiceClip：**整句播放一次**的语音。本句有语音 → 播（并打断上一句，避免两条叠着响）；
///     **本句没有语音 → 默认让上一句播完**（见 <c>previousVoice</c>）——
///     因为"文字先显示完、语音还没说完"是常态，文字显示完就把语音掐掉会很突兀。
///     要立刻停掉就把 <c>previousVoice</c> 设为 <c>Stop</c>。
///
/// 「等语音结束」同理（<c>waitUntilVoiceEnded</c> + <c>voiceFinished</c> 输出端口，**默认关闭**）：
///   关闭 = 文字显示完链就继续，语音在后台播完；打开 = 等完打字再等语音播完才继续；
///   或关掉它、把 <c>voiceFinished</c> 接到「流程/等待条件」→ 白拿 timeout 与 timeoutTo 超时分支。
///
/// 目标物体由「目标物体」输入端口提供（接 取值/获取物体）。
///
/// ⚠️ shouldWait / dialogueBox 是节点实例状态：同一张图被多条链并发跑过本节点时会互相覆盖
///    （与 ForLoopNode.index、MoveToNode.targetTransform 等共享节点状态的性质相同）。
/// </summary>
[CreateNodeMenu("对话UI/台词")]
[NodeTint("#FF99CC")]
[NodeWidth(300)]
public class SetDialogueLineNode : ComponentActionNode<DialogueBoxController>
{
    [Header("对话文本（可接 字符串 节点）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string dialogueText;

    [Header("打字机逐字音效（留空 = 不发声）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public AudioClip dialogueAudioClip;

    [Header("整句语音（留空 = 不发声）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public AudioClip voiceClip;

    [Header("设置后播放语音（语音留空时无效）")]
    public bool playVoice = true;

    [Header("本句没有语音时，上一句还没播完的语音")]
    [Tooltip("让它播完（默认）：文字显示完不等于语音说完，不该掐掉。\n" +
             "立刻停掉：下一句一出现就掐掉上一句的语音。\n" +
             "（本句**有**语音时总是先停掉上一句再播新的 —— 否则两条语音会叠着响。）")]
    public PreviousVoiceMode previousVoice = PreviousVoiceMode.LetFinish;

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

    [Header("等语音播完再走下一个节点")]
    [Tooltip("勾选：本节点等完打字之后，**再等语音播完**才继续（仅在本句「有语音且勾了播放」时有意义）。\n" +
             "默认关闭 = 不改变现有行为：文字显示完链就继续，语音在后台播完。\n" +
             "取消它并把「语音已结束」输出端口接到「流程/等待条件」→ 白拿 timeout 与 timeoutTo 超时分支。")]
    public bool waitUntilVoiceEnded = false;

    [Header("语音已结束（配合 流程/等待条件 可带超时）")]
    [Output]
    public bool voiceFinished;

    [Header("打字已结束（配合 流程/等待条件 可带超时）")]
    [Output]
    public bool typingFinished;

    // 本次执行是否要等（Apply 里确定）
    private bool shouldWait;
    private bool shouldWaitVoice;
    private DialogueBoxController dialogueBox;

    /// <summary>每次执行先清掉上一次的状态，避免目标解析失败时沿用陈旧状态而错误等待</summary>
    public override void Execute()
    {
        shouldWait = false;
        shouldWaitVoice = false;
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

        // ---- 两个音频：都留空就不发声（组件两端都有判空） ----
        AudioClip sfx = GetInputValue<AudioClip>(nameof(dialogueAudioClip), dialogueAudioClip);
        AudioClip voice = GetInputValue<AudioClip>(nameof(voiceClip), voiceClip);
        box.DialogueAudioClip = sfx;   // 逐字音效：设空 = 关闭
        box.VoiceClip = voice;

        bool start = GetInputValue<bool>(nameof(startTypewriter), startTypewriter);
        if (start)
        {
            box.StartDialogue();
        }

        // 语音：
        //   · 本句有片段 → 播（PlayVoice 会先停掉上一句，否则两条语音叠着响）；
        //   · 本句没片段 → 默认**不动**上一句，让它播完（文字显示完 ≠ 语音说完）；
        //     只有显式选了 Stop 才掐掉。
        bool play = GetInputValue<bool>(nameof(playVoice), playVoice);
        if (voice != null)
        {
            if (play) box.PlayVoice();
        }
        else if (previousVoice == PreviousVoiceMode.Stop)
        {
            box.StopVoice();
        }

        dialogueBox = box;
        shouldWait = waitUntilTyped && start;
        shouldWaitVoice = waitUntilVoiceEnded && voice != null && play;   // 只有真的播了才需要等

        string voiceDesc = voice == null
            ? (previousVoice == PreviousVoiceMode.Stop ? "无（已停掉上一句语音）" : "无（上一句语音继续播完）")
            : (play ? $"播放 {voice.name}" : $"{voice.name}（未播放）");
        NodeLog.Info(
            $"{GetType().Name}: 台词已设置（{(content != null ? content.Length : 0)} 字），" +
            $"打字机{(start ? "已启动" : "未启动")}，逐字音效 {(sfx != null ? sfx.name : "无")}，语音 {voiceDesc}" +
            $"{(shouldWait ? "，等待打字结束" : "")}{(shouldWaitVoice ? "，等待语音结束" : "")}");
    }

    /// <summary>
    /// 等打字结束：TMPWriter 启动后 IsWriting 立即为 true（OnStartWriting 在协程第一个 yield 之前同步执行），
    /// 逐字显示完变 false —— 所以逐帧轮询即可，不存在"启动前就判定为结束"的时序竞态。
    ///
    /// 等语音结束：顺序是**先文字后语音**；<c>IsVoicePlaying</c> 里带了一帧容错，
    /// 所以音频刚开播的那一帧不会被误判成"已结束"（详见 DialogueBoxController.IsVoicePlaying）。
    /// </summary>
    public override IEnumerator GetFlow()
    {
        if (dialogueBox == null) yield break;

        // ① 等打字结束
        if (shouldWait)
        {
            while (dialogueBox.IsTyping)
            {
                yield return null;
            }
        }

        // ② 等语音结束
        if (shouldWaitVoice)
        {
            while (dialogueBox.IsVoicePlaying)
            {
                yield return null;
            }
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(dialogueText))
            return GetInputValue<string>(nameof(dialogueText), dialogueText);

        if (port.fieldName == nameof(dialogueAudioClip))
            return GetInputValue<AudioClip>(nameof(dialogueAudioClip), dialogueAudioClip);

        if (port.fieldName == nameof(voiceClip))
            return GetInputValue<AudioClip>(nameof(voiceClip), voiceClip);

        if (port.fieldName == nameof(startTypewriter))
            return GetInputValue<bool>(nameof(startTypewriter), startTypewriter);

        if (port.fieldName == nameof(voiceFinished))
        {
            // 与 typingFinished 同极性：true = 已结束。
            // dialogueBox == null（本节点还没执行过 / 目标没解析到）→ 视为"已结束"：fail-open，避免卡死剧情。
            voiceFinished = dialogueBox == null || !dialogueBox.IsVoicePlaying;
            return voiceFinished;
        }

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
