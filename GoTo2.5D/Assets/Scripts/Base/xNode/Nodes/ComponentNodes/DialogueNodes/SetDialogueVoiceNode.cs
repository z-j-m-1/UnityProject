using UnityEngine;
using XNode;

/// <summary>
/// 对话UI-语音：设置 <see cref="DialogueBoxController"/> 的**对话语音**（整句播放一次），
/// 与「对话UI/对话音效」区分 —— 后者是打字机**逐字**播放的短音效。
///
/// 用法：默认「设置后立刻播放」；把语音设为「空」则清空并**停掉**当前正在播的语音。
/// **没有语音片段就不发声**：本节点与组件两端都做了判空，不会报错、也不会去创建音频管理对象。
/// 目标物体由「目标物体」输入端口提供（接 取值/获取物体 系列节点）。
/// </summary>
[CreateNodeMenu("对话UI/语音")]
[NodeTint("#FF99CC")]
public class SetDialogueVoiceNode : ComponentActionNode<DialogueBoxController>
{
    [Header("对话语音（整句播放一次）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public AudioClip voiceClip;

    [Header("设置后立刻播放（关掉则只设置，稍后再播）")]
    public bool playNow = true;

    protected override void Apply(DialogueBoxController box)
    {
        AudioClip clip = GetInputValue<AudioClip>(nameof(voiceClip), voiceClip);
        box.VoiceClip = clip;

        if (clip == null)
        {
            // 设为空 = 关闭语音，并停掉正在播的那条
            box.StopVoice();
            NodeLog.Info($"{GetType().Name}: 语音已清空（不发声）");
            return;
        }

        if (playNow)
        {
            box.PlayVoice();
            NodeLog.Info($"{GetType().Name}: 播放语音 '{clip.name}'");
        }
        else
        {
            NodeLog.Info($"{GetType().Name}: 语音 = '{clip.name}'（未播放）");
        }
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(voiceClip))
        {
            return GetInputValue<AudioClip>(nameof(voiceClip), voiceClip);
        }
        return null;
    }
}
