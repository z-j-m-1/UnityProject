using UnityEngine;
using XNode;

/// <summary>
/// 音频-音乐音量：设置 MusicManager 单例的音乐音量（0~1，内部会 Clamp）。
/// 音量是输入端口，可接线做渐变 —— 如接「数学运算/浮点运算」逐步调低实现淡出。不依赖目标物体。
/// </summary>
[CreateNodeMenu("音频/音乐音量")]
public class SetMusicVolumeNode : FlowNode
{
    [Header("音乐音量（0~1）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float volume = 1f;

    public override void Execute()
    {
        float v = GetInputValue<float>(nameof(volume), volume);
        MusicManager.Instance.SetMusicVolume(v);
        NodeLog.Info($"{GetType().Name}: 音乐音量 = {Mathf.Clamp01(v)}");
    }
}
