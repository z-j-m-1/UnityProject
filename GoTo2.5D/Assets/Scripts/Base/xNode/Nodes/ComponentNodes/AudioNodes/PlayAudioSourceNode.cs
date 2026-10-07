using UnityEngine;
using XNode;

/// <summary>播放方式</summary>
public enum AudioSourcePlayMode
{
    /// <summary>source.Play()：受音源自身的 loop / volume / priority 等设置影响，播完按音源设置循环</summary>
    Play,
    /// <summary>source.PlayOneShot(clip)：播一次且**可与其它声音叠加**，不受 loop 影响</summary>
    PlayOneShot
}

/// <summary>
/// 音频-播放(目标音源)：播放**目标物体自带 AudioSource**（与走全局单例的「音频/播放音效」区分开）。
/// 适合需要空间化 / 挂在物体上的音源（3D 音效、机关音、脚步等）。
///
/// · <c>Play</c>：剪辑可留空 → 直接用音源上已配置的 clip；填了则先替换 clip 再播。
/// · <c>PlayOneShot</c>：必须指定剪辑。
///
/// 与「音频/停止」(<c>StopAudioNode</c>，作用于同一个目标音源) 配对使用。
/// </summary>
[CreateNodeMenu("音频/播放(目标音源)")]
public class PlayAudioSourceNode : ComponentActionNode<AudioSource>
{
    [Header("播放方式")]
    public AudioSourcePlayMode mode = AudioSourcePlayMode.Play;

    [Header("剪辑（Play 可留空 = 用音源自带的；PlayOneShot 必填）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public AudioClip clip;

    protected override void Apply(AudioSource source)
    {
        AudioClip c = GetInputValue<AudioClip>(nameof(clip), clip);

        if (mode == AudioSourcePlayMode.PlayOneShot)
        {
            if (c == null)
            {
                NodeLog.Warning($"{GetType().Name}: PlayOneShot 必须指定剪辑");
                return;
            }

            source.PlayOneShot(c);
            NodeLog.Info($"{GetType().Name}: PlayOneShot '{c.name}' @ '{source.name}'");
            return;
        }

        if (c != null)
        {
            source.clip = c;
        }

        if (source.clip == null)
        {
            NodeLog.Warning($"{GetType().Name}: 音源 '{source.name}' 没有剪辑，且本节点未指定剪辑");
            return;
        }

        source.Play();
        NodeLog.Info($"{GetType().Name}: Play '{source.clip.name}' @ '{source.name}'");
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(clip))
            return GetInputValue<AudioClip>(nameof(clip), clip);
        return null;
    }
}
