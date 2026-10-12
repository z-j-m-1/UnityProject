using UnityEngine;
using XNode;

/// <summary>
/// 音频-语音音量：设置 MusicManager 单例的语音音量（0~1，内部会 Clamp）。不依赖目标物体。
/// 与「音频/音乐音量」「音频/音效音量」并列 —— 语音是**独立的一条音源**，所以有独立音量。
/// </summary>
[CreateNodeMenu("音频/语音音量")]
public class SetVoiceVolumeNode : FlowNode
{
    [Header("语音音量（0~1）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float volume = 1f;

    public override void Execute()
    {
        float v = GetInputValue<float>(nameof(volume), volume);
        MusicManager.Instance.SetVoiceVolume(v);
        NodeLog.Info($"{GetType().Name}: 语音音量 = {Mathf.Clamp01(v)}");
    }
}
