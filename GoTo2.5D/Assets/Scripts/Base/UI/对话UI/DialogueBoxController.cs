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