using UnityEngine;
using XNode;

/// <summary>对话UI-获取边框图片：读取 DialogueBoxController 的边框 Sprite。</summary>
[CreateNodeMenu("对话UI/获取边框图片")]
[NodeTint("#FF99CC")]
public class GetDialogueBorderSpriteNode : GetDialogueNodeBase
{
    [Header("边框图片")]
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public Sprite borderSprite;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName != nameof(borderSprite))
        {
            return null;
        }

        DialogueBoxController box = ResolveController();
        borderSprite = box != null ? box.BorderSprite : null;
        return borderSprite;
    }
}
