using UnityEngine;
using XNode;

/// <summary>音频-停止（AudioSource）</summary>
[CreateNodeMenu("音频/停止")]
public class StopAudioNode : ComponentActionNode<AudioSource>
{
    protected override void Apply(AudioSource source)
    {
        source.Stop();
    }
}
