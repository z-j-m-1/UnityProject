# 音乐系统

## MusicManager（单例）

访问 `Instance` 时自动生成 GameObject 并 `DontDestroyOnLoad`。

| 方法 | 说明 |
|---|---|
| `PlayMusic(AudioClip clip, bool loop = true)` | 播放背景音乐 |
| `StopMusic()` | 停止音乐 |
| `PlaySFX(AudioClip clip)` | 播放音效 |
| `SetMusicVolume(float volume)` | 设置音乐音量（0~1） |
| `SetSFXVolume(float volume)` | 设置音效音量（0~1） |
| `PlayClickSFX()` | 点击音效（可扩展默认音效） |

## 使用

```csharp
MusicManager.Instance.PlayMusic(musicClip);
MusicManager.Instance.PlaySFX(sfxClip);
```
