using UnityEngine;
using XNode;

/// <summary>
/// 音频-播放音乐：通过 MusicManager 单例的音乐音源播放（单轨，同一时刻只有一首）。
/// 不依赖目标物体（FlowNode，非组件节点）。
/// </summary>
[CreateNodeMenu("音频/播放音乐")]
public class PlayMusicNode : FlowNode
{
    [Header("音乐剪辑")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public AudioClip clip;

    [Header("循环")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public bool loop = true;

    public override void Execute()
    {
        AudioClip c = GetInputValue<AudioClip>(nameof(clip), clip);
        if (c == null)
        {
            NodeLog.Warning($"{GetType().Name}: 音乐剪辑为空");
            return;
        }

        bool lp = GetInputValue<bool>(nameof(loop), loop);
        MusicManager.Instance.PlayMusic(c, lp);
        NodeLog.Info($"{GetType().Name}: 播放音乐 '{c.name}'（循环={lp}）");
    }
}
