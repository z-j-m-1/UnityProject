using UnityEngine;
using XNode;

/// <summary>音频-播放（AudioSource）</summary>
[CreateNodeMenu("音频/播放")]
public class PlayAudioNode : ComponentActionNode<AudioSource>
{
    protected override void Apply(AudioSource source)
    {
        source.Play();
    }
}
