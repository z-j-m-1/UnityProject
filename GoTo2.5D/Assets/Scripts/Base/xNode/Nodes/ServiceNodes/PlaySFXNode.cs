using UnityEngine;
using XNode;

/// <summary>
/// 音频-播放音效：通过 MusicManager 单例的共享 SFX 音源播放（PlayOneShot，可叠加）。
/// 不依赖目标物体（FlowNode，非组件节点）；适合 UI 音效 / 短音效 / 不关心位置的音。
/// 若想播放"目标物体自带的 AudioSource"，请用「音频/播放」（PlayAudioNode）。
/// </summary>
[CreateNodeMenu("音频/播放音效")]
public class PlaySFXNode : FlowNode
{
    [Header("音效剪辑")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public AudioClip clip;

    public override void Execute()
    {
        AudioClip c = GetInputValue<AudioClip>(nameof(clip), clip);
        if (c == null)
        {
            NodeLog.Warning($"{GetType().Name}: 音效剪辑为空");
            return;
        }

        MusicManager.Instance.PlaySFX(c);
        NodeLog.Info($"{GetType().Name}: 播放音效 '{c.name}'");
    }
}
