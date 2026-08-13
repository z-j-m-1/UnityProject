using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Cinemachine;

public class NameSelectPanel : MonoBehaviour
{
    // 单例实例
    public static NameSelectPanel Instance { get; private set; }

    // 单词预设体
    [SerializeField] private GameObject wordPrefab;

    // 单词父物体
    [SerializeField] private Transform wordsParent;

    [SerializeField] private AudioClip SaydAudioClip;

    // 名称变化事件 - 外部通过这个事件来更新UI
    [SerializeField] private UnityEvent<string> OnNameChanged;

    [SerializeField] private CinemachineVirtualCamera OneCamera;
    [SerializeField] private CinemachineVirtualCamera TwoCamera;

    private CinemachineVirtualCamera currentCamera;

    // 玩家名字
    private string _playerName = "";
    public string PlayerName
    {
        get { return _playerName; }
        private set
        {
            if (_playerName != value)
            {
                _playerName = value;

                // 触发名称变化事件，外部监听者（如Text组件）通过这个事件更新显示
                OnNameChanged?.Invoke(_playerName);
            }
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 初始化时触发一次，让外部更新显示
        OnNameChanged?.Invoke(_playerName);

        currentCamera = OneCamera;

        GenerateAlphabetWords();
    }

    // 生成26个英文字母
    private void GenerateAlphabetWords()
    {
        if (wordPrefab == null || wordsParent == null) return;

        TextMeshProUGUI prefabText = wordPrefab.GetComponentInChildren<TextMeshProUGUI>();
        if (prefabText == null) return;

        string originalText = prefabText.text;

        for (int i = 0; i < 26; i++)
        {
            char letter = (char)('A' + i);
            prefabText.text = letter.ToString();
            GameObject newWord = Instantiate(wordPrefab, wordsParent);

            Button btn = newWord.GetComponent<Button>();
            if (btn != null)
            {
                string capturedLetter = letter.ToString();
                btn.onClick.AddListener(() => AddWord(capturedLetter));
            }
        }

        prefabText.text = originalText;
    }

    // 添加单词
    public void AddWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return;

        if (string.IsNullOrEmpty(_playerName))
        {
            PlayerName = word;
        }
        else
        {
            PlayerName = _playerName  + word;
        }
    }

    /// <summary>
    /// 删除最后一个字符
    /// </summary>
    public void DeleteLastCharacter()
    {
        if (string.IsNullOrEmpty(_playerName)) return;
        PlayerName = _playerName.Substring(0, _playerName.Length - 1);
    }

    // 清空名字
    public void ClearName()
    {
        PlayerName = "";
    }

    public void OnConfirmButtonClicked()
    {
        if (string.IsNullOrEmpty(_playerName))
        {
            Debug.LogWarning("Player name is empty. Please enter a name.");
            return;
        }

        PlayerPrefs.SetString("PlayerName", _playerName);
        PlayerPrefs.Save();

        Debug.Log($"Player name '{_playerName}' saved to PlayerPrefs.");
    }

    public void PlaySaydAudio()
    {
        MusicManager.Instance.PlaySFX(SaydAudioClip);
    }

    public void SwitchCamera()
    {
        if (currentCamera == OneCamera)
        {
            TwoCamera.Priority = 11;
            OneCamera.Priority = 10;
            currentCamera = TwoCamera;
        }
        else
        {
            OneCamera.Priority = 11;
            TwoCamera.Priority = 10;
            currentCamera = OneCamera;
        }
    }
}