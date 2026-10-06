using UnityEngine;
using XNode;

/// <summary>音频-播放（AudioSource）</summary>
[CreateNodeMenu("音频/播放")]
public class PlayAudioNode : ComponentActionNode<AudioSource>
{
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public AudioClip clip;
    protected override void Apply(AudioSource component)
    {
        MusicManager.Instance.PlaySFX(GetInputValue<AudioClip>(nameof(clip), clip));
    }
}
