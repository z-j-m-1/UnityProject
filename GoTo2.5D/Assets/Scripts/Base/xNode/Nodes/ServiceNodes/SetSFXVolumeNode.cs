using UnityEngine;
using XNode;

/// <summary>
/// 音频-音效音量：设置 MusicManager 单例的音效音量（0~1，内部会 Clamp）。不依赖目标物体。
/// </summary>
[CreateNodeMenu("音频/音效音量")]
public class SetSFXVolumeNode : FlowNode
{
    [Header("音效音量（0~1）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float volume = 1f;

    public override void Execute()
    {
        float v = GetInputValue<float>(nameof(volume), volume);
        MusicManager.Instance.SetSFXVolume(v);
        NodeLog.Info($"{GetType().Name}: 音效音量 = {Mathf.Clamp01(v)}");
    }
}
