using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TMPEffects.Components;

[RequireComponent(typeof(TMPWriter))]
[RequireComponent(typeof(TMPAnimator))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class DialogueBoxController : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    private TMPWriter tmpWriter;
    private TMPAnimator tmpAnimator;

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

    #endregion

    void Awake()
    {
        tmpWriter = GetComponent<TMPWriter>();
        tmpAnimator = GetComponent<TMPAnimator>();

        if (dialogueText == null)
            dialogueText = GetComponent<TextMeshProUGUI>();
    }

    // 驱动 TMPWriter 开始显示对话内容
    public void StartDialogue()
    {
        if (tmpWriter != null)
            tmpWriter.StartWriter();
    }
}