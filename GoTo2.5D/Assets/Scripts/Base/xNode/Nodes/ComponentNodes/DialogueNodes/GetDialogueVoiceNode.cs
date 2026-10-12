using UnityEngine;
using XNode;

/// <summary>对话UI-获取语音：读取 DialogueBoxController 当前配置的语音剪辑（为空表示不发声）。</summary>
[CreateNodeMenu("对话UI/获取语音")]
[NodeTint("#FF99CC")]
public class GetDialogueVoiceNode : GetDialogueNodeBase
{
    [Header("语音剪辑")]
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public AudioClip voiceClip;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName != nameof(voiceClip))
        {
            return null;
        }

        DialogueBoxController box = ResolveController();
        voiceClip = box != null ? box.VoiceClip : null;
        return voiceClip;
    }
}
