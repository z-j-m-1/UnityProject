using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 收集者 - 挂在 Canvas 上，收集其下所有 Text / TextMeshPro / Image 组件（按物体名索引）
/// 同类型同名会互相覆盖，收集时打警告提醒。不 DontDestroyOnLoad，随场景重建。
/// </summary>
public class UICollector : MonoBehaviour
{
    private static UICollector _instance;

    private readonly Dictionary<string, Text> texts = new Dictionary<string, Text>();
    private readonly Dictionary<string, TMP_Text> tmpTexts = new Dictionary<string, TMP_Text>();
    private readonly Dictionary<string, Image> images = new Dictionary<string, Image>();

    /// <summary>
    /// UI 收集者单例（自动找场景 Canvas 挂载；没有 Canvas 则创建）
    /// </summary>
    public static UICollector Instance
    {
        get
        {
            if (_instance == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    GameObject go = new GameObject("Canvas");
                    canvas = go.AddComponent<Canvas>();
                    go.AddComponent<CanvasScaler>();
                    go.AddComponent<GraphicRaycaster>();
                }
                _instance = canvas.gameObject.AddComponent<UICollector>();
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }
        _instance = this;
        Refresh();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    /// <summary>重新收集 Canvas 下的所有 UI 组件</summary>
    public void Refresh()
    {
        texts.Clear();
        tmpTexts.Clear();
        images.Clear();

        Text[] textComps = GetComponentsInChildren<Text>(true);
        foreach (Text t in textComps)
        {
            AddToDict(texts, t.gameObject.name, t);
        }

        TMP_Text[] tmpComps = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in tmpComps)
        {
            AddToDict(tmpTexts, t.gameObject.name, t);
        }

        Image[] imageComps = GetComponentsInChildren<Image>(true);
        foreach (Image img in imageComps)
        {
            AddToDict(images, img.gameObject.name, img);
        }
    }

    private static void AddToDict<T>(Dictionary<string, T> dict, string name, T component) where T : Component
    {
        if (string.IsNullOrEmpty(name) || component == null) return;
        if (dict.ContainsKey(name))
        {
            Debug.LogWarning($"UICollector: 检测到同类型同名 UI 组件 '{name}'（{typeof(T).Name}），后者覆盖前者，请避免 UI 重名");
        }
        dict[name] = component;
    }

    /// <summary>按类型 + 物体名查找组件</summary>
    public T Find<T>(string name) where T : Component
    {
        if (string.IsNullOrEmpty(name)) return null;

        if (typeof(T) == typeof(Text))
        {
            texts.TryGetValue(name, out Text value);
            return value as T;
        }
        if (typeof(T) == typeof(TMP_Text))
        {
            tmpTexts.TryGetValue(name, out TMP_Text value);
            return value as T;
        }
        if (typeof(T) == typeof(Image))
        {
            images.TryGetValue(name, out Image value);
            return value as T;
        }
        return null;
    }
}
