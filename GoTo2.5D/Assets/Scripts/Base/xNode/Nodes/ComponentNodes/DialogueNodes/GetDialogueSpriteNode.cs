using UnityEngine;
using XNode;

/// <summary>
/// 对话UI-获取图片：**一个节点读取三张图片之一**（槽位枚举：边框 / 背景 / 人物），
/// 原「对话UI/获取边框图片」「对话UI/获取背景图片」「对话UI/获取人物图片」三个逐行同构的节点已合并删除。
/// 与设置侧的「对话UI/图片」共用同一个 <see cref="DialogueImageSlot"/> 枚举。
/// </summary>
[CreateNodeMenu("对话UI/获取图片")]
[NodeTint("#FF99CC")]
public class GetDialogueSpriteNode : GetDialogueNodeBase
{
    [Header("图片槽位")]
    public DialogueImageSlot slot = DialogueImageSlot.Border;

    [Header("图片")]
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public Sprite sprite;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName != nameof(sprite))
        {
            return null;
        }

        DialogueBoxController box = ResolveController();
        if (box == null)
        {
            sprite = null;
            return sprite;
        }

        switch (slot)
        {
            case DialogueImageSlot.Background:
                sprite = box.BackgroundSprite;
                break;

            case DialogueImageSlot.Character:
                sprite = box.CharacterSprite;
                break;

            default:
                sprite = box.BorderSprite;
                break;
        }

        return sprite;
    }
}
