using UnityEngine;
using XNode;

/// <summary>音频-停止音乐：停止 MusicManager 单例正在播放的音乐（不影响音效）。不依赖目标物体。</summary>
[CreateNodeMenu("音频/停止音乐")]
public class StopMusicNode : FlowNode
{
    public override void Execute()
    {
        MusicManager.Instance.StopMusic();
        NodeLog.Info($"{GetType().Name}: 已停止音乐");
    }
}
