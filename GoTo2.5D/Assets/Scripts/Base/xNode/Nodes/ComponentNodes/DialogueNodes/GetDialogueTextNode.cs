using UnityEngine;
using XNode;

/// <summary>对话UI-获取对话内容：读取 DialogueBoxController 当前的对话文本。</summary>
[CreateNodeMenu("对话UI/获取对话内容")]
[NodeTint("#FF99CC")]
public class GetDialogueTextNode : GetDialogueNodeBase
{
    [Header("对话内容")]
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public string dialogueText;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName != nameof(dialogueText))
        {
            return null;
        }

        DialogueBoxController box = ResolveController();
        dialogueText = box != null ? box.DialogueText : null;
        return dialogueText;
    }
}
