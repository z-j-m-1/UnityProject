using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UICommunicator : MonoBehaviour
{
    // 缓存结构：图名称 -> (UI对象名称 -> UIComponentCache)
    private Dictionary<string, Dictionary<string, UIComponentCache>> uiCacheDict = new Dictionary<string, Dictionary<string, UIComponentCache>>();

    // 正则表达式匹配所有HTML/XML标签（在事件中使用）
    private static readonly Regex richTextRegex = new Regex("<.*?>", RegexOptions.Compiled);

    private static UICommunicator _instance;
    public static UICommunicator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UICommunicator>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(UICommunicator).Name);
                    _instance = go.AddComponent<UICommunicator>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Start()
    {
        ComGetSelfTextMeshProEvent.Subscribe(OnComGetSelfTextMeshPro);
        ComSetSelfTextMeshProEvent.Subscribe(OnComSetSelfTextMeshPro);
        ComGetSelfTextEvent.Subscribe(OnComGetSelfText);
        ComSetSelfTextEvent.Subscribe(OnComSetSelfText);
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        ComGetSelfTextMeshProEvent.Unsubscribe(OnComGetSelfTextMeshPro);
        ComSetSelfTextMeshProEvent.Unsubscribe(OnComSetSelfTextMeshPro);
        ComGetSelfTextEvent.Unsubscribe(OnComGetSelfText);
        ComSetSelfTextEvent.Unsubscribe(OnComSetSelfText);
    }

    private void OnSceneUnloaded(Scene scene)
    {
        ClearAll();
    }

    // ============ 缓存管理 ============

    public UIComponentCache GetOrCreateCache(string graphName, string uiObjectName, GameObject attachedObject)
    {
        if (string.IsNullOrEmpty(graphName) || string.IsNullOrEmpty(uiObjectName) || attachedObject == null)
            return null;

        if (!uiCacheDict.TryGetValue(graphName, out Dictionary<string, UIComponentCache> graphCache))
        {
            graphCache = new Dictionary<string, UIComponentCache>();
            uiCacheDict.Add(graphName, graphCache);
        }

        if (graphCache.TryGetValue(uiObjectName, out UIComponentCache cache))
        {
            return cache;
        }

        GameObject uiObject = null;

        if (attachedObject.name == uiObjectName)
        {
            uiObject = attachedObject;
            Debug.Log($"UICommunicator: 找到自身UI对象 '{uiObjectName}'");
        }
        else
        {
            Transform uiTransform = attachedObject.transform.Find(uiObjectName);
            if (uiTransform != null)
            {
                uiObject = uiTransform.gameObject;
                Debug.Log($"UICommunicator: 找到子UI对象 '{uiObjectName}'");
            }
        }

        if (uiObject == null)
        {
            Debug.LogError($"UICommunicator: 未找到UI对象 '{uiObjectName}'（自身或子物体）");
            return null;
        }

        cache = new UIComponentCache(uiObject);
        graphCache.Add(uiObjectName, cache);
        Debug.Log($"UICommunicator: 缓存UI对象 '{graphName}.{uiObjectName}'");

        return cache;
    }

    private UIComponentCache GetCache(string graphName, string uiObjectName)
    {
        if (string.IsNullOrEmpty(graphName) || string.IsNullOrEmpty(uiObjectName))
            return null;

        if (uiCacheDict.TryGetValue(graphName, out Dictionary<string, UIComponentCache> graphCache))
        {
            if (graphCache.TryGetValue(uiObjectName, out UIComponentCache cache))
            {
                return cache;
            }
        }
        return null;
    }

    public void ClearAll()
    {
        uiCacheDict.Clear();
    }

    // ============ TextMeshPro 事件处理 ============

    private void OnComGetSelfTextMeshPro(ComGetSelfTextMeshProEvent evt)
    {
        var cache = GetCache(evt.graphName, evt.uiObjectName);
        if (cache != null)
        {
            // ✅ 在事件中去除富文本
            string rawContent = cache.TextMeshProContent;
            string plainContent = richTextRegex.Replace(rawContent, string.Empty);
            evt.callback?.Invoke(plainContent);
        }
        else
        {
            Debug.LogError($"UICommunicator: 未找到UI对象 '{evt.graphName}.{evt.uiObjectName}'");
            evt.callback?.Invoke(evt.defaultValue);
        }
    }

    private void OnComSetSelfTextMeshPro(ComSetSelfTextMeshProEvent evt)
    {
        var cache = GetCache(evt.graphName, evt.uiObjectName);
        if (cache != null)
        {
            cache.TextMeshProContent = evt.textValue;
            Debug.Log($"UICommunicator: 设置TextMeshPro '{evt.graphName}.{evt.uiObjectName}' = '{evt.textValue}'");
        }
        else
        {
            Debug.LogError($"UICommunicator: 未找到UI对象 '{evt.graphName}.{evt.uiObjectName}'");
        }
    }

    private void OnComGetSelfText(ComGetSelfTextEvent evt)
    {
        var cache = GetCache(evt.graphName, evt.uiObjectName);
        if (cache != null)
        {
            // Text不支持富文本，直接返回
            string content = cache.TextContent;
            Debug.Log($"UICommunicator: 获取Text '{evt.graphName}.{evt.uiObjectName}' = '{content}'");
            evt.callback?.Invoke(content);
        }
        else
        {
            Debug.LogError($"UICommunicator: 未找到UI对象 '{evt.graphName}.{evt.uiObjectName}'");
            evt.callback?.Invoke(evt.defaultValue);
        }
    }

    private void OnComSetSelfText(ComSetSelfTextEvent evt)
    {
        var cache = GetCache(evt.graphName, evt.uiObjectName);
        if (cache != null)
        {
            cache.TextContent = evt.textValue;
            Debug.Log($"UICommunicator: 设置Text '{evt.graphName}.{evt.uiObjectName}' = '{evt.textValue}'");
        }
        else
        {
            Debug.LogError($"UICommunicator: 未找到UI对象 '{evt.graphName}.{evt.uiObjectName}'");
        }
    }
}

public class UIComponentCache
{
    private GameObject attachedObject;
    private TextMeshProUGUI textMeshPro;
    private Text text;

    public GameObject AttachedObject => attachedObject;

    public UIComponentCache(GameObject obj)
    {
        UpdateAttachedObject(obj);
    }

    public void UpdateAttachedObject(GameObject obj)
    {
        attachedObject = obj;
        if (obj != null)
        {
            textMeshPro = obj.GetComponent<TextMeshProUGUI>();
            text = obj.GetComponent<Text>();
        }
        else
        {
            textMeshPro = null;
            text = null;
        }
    }

    // ============ TextMeshPro ============

    /// <summary>
    /// 原始TextMeshPro内容（包含富文本标签）
    /// </summary>
    public string TextMeshProContent
    {
        get => textMeshPro != null ? textMeshPro.text : string.Empty;
        set { if (textMeshPro != null) textMeshPro.text = value; }
    }

    // ============ Text ============

    public string TextContent
    {
        get => text != null ? text.text : string.Empty;
        set { if (text != null) text.text = value; }
    }

    public bool HasTextMeshPro => textMeshPro != null;
    public bool HasText => text != null;
}