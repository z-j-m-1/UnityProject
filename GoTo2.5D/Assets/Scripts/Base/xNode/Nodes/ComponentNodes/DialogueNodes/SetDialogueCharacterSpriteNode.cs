using UnityEngine;
using XNode;

/// <summary>对话UI-设置人物图片：修改 DialogueBoxController 的人物 Image 的 Sprite（立绘/头像）。</summary>
[CreateNodeMenu("对话UI/人物图片")]
[NodeTint("#FF99CC")]
public class SetDialogueCharacterSpriteNode : ComponentActionNode<DialogueBoxController>
{
    [Header("人物图片（Sprite，可接线）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Sprite sprite;

    protected override void Apply(DialogueBoxController box)
    {
        Sprite value = GetInputValue<Sprite>(nameof(sprite), sprite);
        box.CharacterSprite = value;

        // 读回校验：写入非空却读不到 → 目标上的人物 Image 引用未配置，赋值被静默忽略
        if (value != null && box.CharacterSprite == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{box.name}' 的人物图片引用未配置，设置被忽略");
            return;
        }

        NodeLog.Info($"{GetType().Name}: 人物图片 = {(value != null ? value.name : "空")}");
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(sprite))
            return GetInputValue<Sprite>(nameof(sprite), sprite);
        return null;
    }
}
