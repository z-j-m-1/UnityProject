using UnityEngine;
using XNode;

/// <summary>对话UI-设置背景图片：修改 DialogueBoxController 的背景 Image 的 Sprite。</summary>
[CreateNodeMenu("对话UI/背景图片")]
[NodeTint("#FF99CC")]
public class SetDialogueBackgroundSpriteNode : ComponentActionNode<DialogueBoxController>
{
    [Header("背景图片（Sprite，可接线）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Sprite sprite;

    protected override void Apply(DialogueBoxController box)
    {
        Sprite value = GetInputValue<Sprite>(nameof(sprite), sprite);
        box.BackgroundSprite = value;

        // 读回校验：写入非空却读不到 → 目标上的背景 Image 引用未配置，赋值被静默忽略
        if (value != null && box.BackgroundSprite == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{box.name}' 的背景图片引用未配置，设置被忽略");
            return;
        }

        NodeLog.Info($"{GetType().Name}: 背景图片 = {(value != null ? value.name : "空")}");
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(sprite))
            return GetInputValue<Sprite>(nameof(sprite), sprite);
        return null;
    }
}
