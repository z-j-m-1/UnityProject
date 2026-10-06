using UnityEngine;
using XNode;

/// <summary>
/// 对话UI-对话音效：设置 DialogueBoxController 的对话音效剪辑（打字机逐字播放的音效）。
/// 设为「空」即关闭逐字音效。目标物体由「目标物体」输入端口提供。
/// </summary>
[CreateNodeMenu("对话UI/对话音效")]
[NodeTint("#FF99CC")]
public class SetDialogueAudioClipNode : ComponentActionNode<DialogueBoxController>
{
    [Header("对话音效（打字机逐字播放）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public AudioClip audioClip;

    protected override void Apply(DialogueBoxController box)
    {
        AudioClip clip = GetInputValue<AudioClip>(nameof(audioClip), audioClip);
        box.DialogueAudioClip = clip;
        NodeLog.Info($"{GetType().Name}: 对话音效 = {(clip != null ? clip.name : "空（已关闭逐字音效）")}");
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(audioClip))
        {
            return GetInputValue<AudioClip>(nameof(audioClip), audioClip);
        }
        return null;
    }
}
