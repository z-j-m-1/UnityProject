using UnityEngine;
using XNode;

/// <summary>对话UI-获取背景图片：读取 DialogueBoxController 的背景 Sprite。</summary>
[CreateNodeMenu("对话UI/获取背景图片")]
[NodeTint("#FF99CC")]
public class GetDialogueBackgroundSpriteNode : GetDialogueNodeBase
{
    [Header("背景图片")]
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public Sprite backgroundSprite;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName != nameof(backgroundSprite))
        {
            return null;
        }

        DialogueBoxController box = ResolveController();
        backgroundSprite = box != null ? box.BackgroundSprite : null;
        return backgroundSprite;
    }
}
