using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TMPEffects.Components;
using TMPEffects.CharacterData;

public class DialogueBoxController : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    [Header("对话音频")]
    [SerializeField] private AudioClip dialogueAudioClip;

    [Header("对话语音（整句播放一次；为空则不发声）")]
    [SerializeField] private AudioClip voiceClip;

    [SerializeField] private TMPWriter tmpWriter;
    [SerializeField] private TMPAnimator tmpAnimator;

    #region 属性包装

    /// <summary>
    /// 对话文本内容
    /// </summary>
    public string DialogueText
    {
        get => dialogueText != null ? dialogueText.text : string.Empty;
        set
        {
            if (dialogueText != null)
                dialogueText.text = value;
        }
    }

    /// <summary>
    /// 边框图片
    /// </summary>
    public Sprite BorderSprite
    {
        get => borderImage != null ? borderImage.sprite : null;
        set
        {
            if (borderImage != null)
                borderImage.sprite = value;
        }
    }

    /// <summary>
    /// 背景图片
    /// </summary>
    public Sprite BackgroundSprite
    {
        get => backgroundImage != null ? backgroundImage.sprite : null;
        set
        {
            if (backgroundImage != null)
                backgroundImage.sprite = value;
        }
    }

    /// <summary>
    /// 对话人物图片
    /// </summary>
    public Sprite CharacterSprite
    {
        get => characterImage != null ? characterImage.sprite : null;
        set
        {
            if (characterImage != null)
                characterImage.sprite = value;
        }
    }

    /// <summary>
    /// 对话音频剪辑
    /// </summary>
    public AudioClip DialogueAudioClip
    {
        get => dialogueAudioClip;
        set => dialogueAudioClip = value;
    }

    /// <summary>
    /// 对话语音剪辑（整句播放一次，区别于 <see cref="DialogueAudioClip"/> 的逐字音效）。
    /// 为空则不发声。
    /// </summary>
    public AudioClip VoiceClip
    {
        get => voiceClip;
        set => voiceClip = value;
    }

    /// <summary>
    /// 打字机是否正在逐字显示（供节点图判断"打字结束"）
    /// </summary>
    public bool IsTyping => tmpWriter != null && tmpWriter.IsWriting;

    #endregion

    void Awake()
    {
        tmpWriter = GetComponent<TMPWriter>();
        tmpAnimator = GetComponent<TMPAnimator>();

        if (dialogueText == null)
            dialogueText = GetComponent<TextMeshProUGUI>();

        // dialogueText 可能仍为 null（物体上没有 TextMeshProUGUI）→ 判空后再从它身上取
        if (dialogueText != null)
        {
            if (tmpWriter == null)
                tmpWriter = dialogueText.GetComponent<TMPWriter>();
            if (tmpAnimator == null)
                tmpAnimator = dialogueText.GetComponent<TMPAnimator>();
        }

        // 逐字音效监听：拿不到 TMPWriter 就跳过（原来这里会直接 NullReferenceException）
        if (tmpWriter != null)
        {
            tmpWriter.OnCharacterShown.AddListener(HandleCharacterShown);
        }
        else
        {
            Debug.LogWarning(
                $"{nameof(DialogueBoxController)}: '{name}' 自身与 TextMeshProUGUI 上都没有 TMPWriter，逐字音效监听已跳过",
                this);
        }
    }

    // 驱动 TMPWriter 开始显示对话内容
    public void StartDialogue()
    {
        if (tmpWriter != null)
            tmpWriter.StartWriter();
    }

    // 语音播放请求帧：-1 = 没有待观察的播放请求（供 IsVoicePlaying 做"头一帧"容错）
    private int voiceRequestFrame = -1;

    /// <summary>
    /// 播放当前对话语音。**没有语音片段就不发声**（直接返回，不报错、也不会去创建音频管理对象）。
    /// 播放会先停掉上一条语音（见 MusicManager.PlayVoice）。
    /// </summary>
    public void PlayVoice()
    {
        if (voiceClip == null) return;
        MusicManager.Instance.PlayVoice(voiceClip);
        voiceRequestFrame = Time.frameCount;
    }

    /// <summary>
    /// 语音是否（仍）在播放。**带一帧容错**：<c>AudioSource.Play()</c> 之后 <c>isPlaying</c>
    /// 有可能会晚一帧才置位 —— 不兜这一帧的话，"等语音播完"会刚开播就判定已结束，等待形同虚设。
    /// 超过一帧仍未置位就承认它确实没在播（所以不会卡死）。
    /// </summary>
    public bool IsVoicePlaying
    {
        get
        {
            if (MusicManager.HasInstance && MusicManager.Instance.IsVoicePlaying) return true;

            return voiceRequestFrame >= 0 && Time.frameCount - voiceRequestFrame <= 1;
        }
    }

    /// <summary>
    /// 停止当前语音（例如玩家跳过这句对话）。
    /// 没创建过音频管理器就**不会**创建它 —— 停语音不该有"凭空建一个管理器"的副作用。
    /// </summary>
    public void StopVoice()
    {
        voiceRequestFrame = -1;

        if (!MusicManager.HasInstance) return;
        MusicManager.Instance.StopVoice();
    }

    /// <summary>
    /// 跳过本句打字：立刻把整段文字显示完（<c>TMPWriter.SkipWriter</c>），并按需停掉语音。
    /// 给"点击跳过"按钮 / 节点用。
    /// </summary>
    public void SkipDialogue(bool stopVoice = true)
    {
        if (tmpWriter != null)
            tmpWriter.SkipWriter();

        if (stopVoice)
            StopVoice();
    }

    /// <summary>
    /// 处理每个字符显示时的事件
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="charData"></param>
    void HandleCharacterShown(TMPWriter writer, CharData charData)
    {
        if (dialogueAudioClip != null)
        {
            MusicManager.Instance.PlaySFX(dialogueAudioClip);
        }
    }
}