using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameFrameX.LitJSON.Runtime;

/// <summary>
/// 存档系统（场景单例）- 负责节点图变量 / 房间变量 / 全局变量的序列化与落盘
/// 可挂到场景中，在 Inspector 右键菜单触发保存/加载/删除
/// </summary>
public class SaveSystem : MonoBehaviour
{
    private const string SaveFileName = "save.json";
    private const int CurrentVersion = 1;

    private static SaveSystem _instance;

    /// <summary>
    /// 存档系统单例（不存在则自动创建）
    /// </summary>
    public static SaveSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(SaveSystem).Name);
                    _instance = go.AddComponent<SaveSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    /// <summary>存档文件完整路径</summary>
    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    /// <summary>是否存在存档</summary>
    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    // ============ 静态入口（内部转调实例，保持原有调用方式） ============

    public static void Save() => Instance.SaveNow();
    public static void Load() => Instance.LoadNow();
    public static void Delete() => Instance.DeleteNow();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ============ 实例方法（Inspector 右键菜单） ============

    /// <summary>
    /// 保存当前状态到存档（仅运行时可调用，合并旧档，仅更新当前场景的房间变量槽与已注册图变量）
    /// </summary>
    [ContextMenu("保存存档")]
    public void SaveNow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("存档系统：请先在运行状态（Play）下保存存档");
            return;
        }

        SaveData data = ReadFromFile() ?? new SaveData();

        data.version = CurrentVersion;
        string sceneName = SceneManager.GetActiveScene().name;
        data.sceneName = sceneName;

        // 全局变量
        data.global = GameGlobalVariableManager.Instance.Export();

        // 房间变量（当前场景槽）
        if (data.roomByScene == null)
        {
            data.roomByScene = new Dictionary<string, VariableBundleData>();
        }
        data.roomByScene[sceneName] = RoomVariableManager.Instance.Export();

        // 节点图变量
        if (data.graphs == null)
        {
            data.graphs = new Dictionary<string, VariableBundleData>();
        }
        foreach (var kvp in GraphCommunicator.Instance.GetAllExecutors())
        {
            BaseNodeGraph graph = kvp.Value != null ? kvp.Value.GetNodeGraph() as BaseNodeGraph : null;
            if (graph == null) continue;

            data.graphs[GetGraphKey(sceneName, graph.name)] = graph.ExportVariables();
        }

        string json = JsonMapper.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"存档系统：已保存存档到 '{SavePath}'");
    }

    /// <summary>
    /// 加载存档（导入全局变量、当前场景房间变量槽、已注册图变量）
    /// </summary>
    [ContextMenu("加载存档")]
    public void LoadNow()
    {
        SaveData data = ReadFromFile();
        if (data == null)
        {
            Debug.Log("存档系统：无存档，使用默认值");
            return;
        }

        // 全局变量
        if (data.global != null)
        {
            GameGlobalVariableManager.Instance.Import(data.global);
        }

        // 房间变量（当前场景槽）
        string sceneName = SceneManager.GetActiveScene().name;
        VariableBundleData room;
        if (data.roomByScene != null && data.roomByScene.TryGetValue(sceneName, out room))
        {
            RoomVariableManager.Instance.Import(room);
        }

        // 节点图变量
        if (data.graphs != null)
        {
            foreach (var kvp in GraphCommunicator.Instance.GetAllExecutors())
            {
                BaseNodeGraph graph = kvp.Value != null ? kvp.Value.GetNodeGraph() as BaseNodeGraph : null;
                if (graph == null) continue;

                VariableBundleData graphData;
                if (data.graphs.TryGetValue(GetGraphKey(sceneName, graph.name), out graphData))
                {
                    graph.ImportVariables(graphData);
                }
            }
        }

        Debug.Log($"存档系统：已加载存档（场景 '{sceneName}'）");
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    [ContextMenu("删除存档")]
    public void DeleteNow()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("存档系统：已删除存档");
        }
    }

    // ============ 私有辅助 ============

    /// <summary>
    /// 生成图变量的存档键：场景名 + "/" + 图资产名（同一张图在不同场景中各自存档）
    /// </summary>
    private static string GetGraphKey(string sceneName, string graphName)
    {
        return sceneName + "/" + graphName;
    }

    /// <summary>
    /// 读取存档文件；不存在或解析失败时返回 null
    /// </summary>
    private static SaveData ReadFromFile()
    {
        if (!File.Exists(SavePath)) return null;

        string json = File.ReadAllText(SavePath);
        try
        {
            return JsonMapper.ToObject<SaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"存档系统：读取存档失败（{e.Message}），将按无存档处理");
            return null;
        }
    }
}
