using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager _instance;

    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MusicManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(MusicManager).Name);
                    _instance = go.AddComponent<MusicManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 是否已存在实例。给"没有副作用需求"的调用用 ——
    /// 例如只想停语音时，不该因为访问 <see cref="Instance"/> 就凭空创建一个音频管理器。
    /// </summary>
    public static bool HasInstance => _instance != null;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource voiceSource;   // 对话语音等：单独一条，才能「先停上一条再播新的」，且不受音效影响

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 自动创建两个音源
        CreateAudioSources();
    }

    // 自动创建音源
    private void CreateAudioSources()
    {
        //确认是否已经存在音源组件
        if (musicSource == null)
        {
            //创建子物体然后添加音源组件
            GameObject musicSourceObj = new GameObject("MusicSource");
            musicSourceObj.transform.SetParent(transform);
            musicSource = musicSourceObj.AddComponent<AudioSource>();
        }
        //确认是否已经存在音源组件
        if (sfxSource == null)
        {
            //创建子物体然后添加音源组件
            GameObject sfxSourceObj = new GameObject("SFXSource");
            sfxSourceObj.transform.SetParent(transform);
            sfxSource = sfxSourceObj.AddComponent<AudioSource>();

            sfxSource.loop = false;
            sfxSource.volume = 1;
            sfxSource.playOnAwake = false;
        }

        // 语音单独一条音源：与音效分开，才能「先停上一条再播新的」
        //（音效走 PlayOneShot 叠加，停不掉；语音不能叠加，否则快速推进对话会糊在一起）
        if (voiceSource == null)
        {
            GameObject voiceSourceObj = new GameObject("VoiceSource");
            voiceSourceObj.transform.SetParent(transform);
            voiceSource = voiceSourceObj.AddComponent<AudioSource>();

            voiceSource.loop = false;
            voiceSource.volume = 1;
            voiceSource.playOnAwake = false;
        }
    }

    // 播放背景音乐
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // 停止音乐
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // 播放音效
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // 播放语音（对话语音等）：**先停掉上一条**再播，避免快速推进对话时多条语音叠在一起。
    // 没有片段就直接返回 —— 不发声，也不报错。
    public void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null || clip == null) return;
        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    // 语音是否正在播放（供「等语音播完」用；没有音源时恒为 false）
    public bool IsVoicePlaying => voiceSource != null && voiceSource.isPlaying;

    // 停止语音
    public void StopVoice()
    {
        if (voiceSource != null) voiceSource.Stop();
    }

    // 设置语音音量
    public void SetVoiceVolume(float volume)
    {
        if (voiceSource != null) voiceSource.volume = Mathf.Clamp01(volume);
    }

    // 设置音乐音量
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    // 设置音效音量
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    // 可选：播放点击音效（方便调用）
    public void PlayClickSFX()
    {
        // 如果需要默认点击音效，可以在这里加载
        // 或者通过PlaySFX传入具体的clip
        // 示例：PlaySFX(clickClip);
    }
}