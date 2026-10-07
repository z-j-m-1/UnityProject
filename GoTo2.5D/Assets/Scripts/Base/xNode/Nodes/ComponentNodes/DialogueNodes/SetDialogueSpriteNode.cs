using UnityEngine;
using XNode;

/// <summary>对话框里的图片槽位</summary>
public enum DialogueImageSlot
{
    Border,
    Background,
    Character
}

/// <summary>
/// 对话UI-设置图片：**一个节点设置三张图片之一**（槽位枚举：边框 / 背景 / 人物），
/// 原「对话UI/边框图片」「对话UI/背景图片」「对话UI/人物图片」三个逐行同构的节点已合并删除。
///
/// 保留了原有的**读回校验**：写入非空却读不回来 → 说明目标上的对应 Image 引用未配置，赋值被静默忽略，这里主动警告。
/// </summary>
[CreateNodeMenu("对话UI/图片")]
[NodeTint("#FF99CC")]
public class SetDialogueSpriteNode : ComponentActionNode<DialogueBoxController>
{
    [Header("图片槽位")]
    public DialogueImageSlot slot = DialogueImageSlot.Border;

    [Header("图片（Sprite，可接线）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Sprite sprite;

    protected override void Apply(DialogueBoxController box)
    {
        Sprite value = GetInputValue<Sprite>(nameof(sprite), sprite);

        string slotName;
        Sprite readBack;

        switch (slot)
        {
            case DialogueImageSlot.Background:
                slotName = "背景";
                box.BackgroundSprite = value;
                readBack = box.BackgroundSprite;
                break;

            case DialogueImageSlot.Character:
                slotName = "人物";
                box.CharacterSprite = value;
                readBack = box.CharacterSprite;
                break;

            default:
                slotName = "边框";
                box.BorderSprite = value;
                readBack = box.BorderSprite;
                break;
        }

        // 读回校验：写入非空却读不到 → 目标上的对应 Image 引用未配置，赋值被静默忽略
        if (value != null && readBack == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{box.name}' 的{slotName}图片引用未配置，设置被忽略");
            return;
        }

        NodeLog.Info($"{GetType().Name}: {slotName}图片 = {(value != null ? value.name : "空")}");
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(sprite))
            return GetInputValue<Sprite>(nameof(sprite), sprite);
        return null;
    }
}
