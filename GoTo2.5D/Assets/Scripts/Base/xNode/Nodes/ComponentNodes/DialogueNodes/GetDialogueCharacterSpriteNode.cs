using UnityEngine;
using XNode;

/// <summary>对话UI-获取人物图片：读取 DialogueBoxController 的人物 Sprite（立绘 / 头像）。</summary>
[CreateNodeMenu("对话UI/获取人物图片")]
[NodeTint("#FF99CC")]
public class GetDialogueCharacterSpriteNode : GetDialogueNodeBase
{
    [Header("人物图片")]
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public Sprite characterSprite;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName != nameof(characterSprite))
        {
            return null;
        }

        DialogueBoxController box = ResolveController();
        characterSprite = box != null ? box.CharacterSprite : null;
        return characterSprite;
    }
}
