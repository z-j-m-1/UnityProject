using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 房间变量管理器（单例）- 持久化房间变量
/// 每次切换场景时，根据场景名称从 Resources/PersistentVariables/Room/ 下加载对应的变量对象
/// </summary>
public class RoomVariableManager : PersistentVariableManager
{
    /// <summary>房间变量对象所在的 Resources 路径</summary>
    private const string RoomVariableFolder = "PersistentVariables/Room/";

    private static RoomVariableManager _instance;

    public static RoomVariableManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RoomVariableManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(RoomVariableManager).Name);
                    _instance = go.AddComponent<RoomVariableManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public override PersistentVariableScope Scope => PersistentVariableScope.Room;

    /// <summary>是否已初始化（供存档系统判断是否需要采集）</summary>
    public static bool IsInitialized => _instance != null;

    /// <summary>各场景登记的节点图：场景名 → 图列表</summary>
    private readonly Dictionary<string, List<BaseNodeGraph>> roomGraphs = new Dictionary<string, List<BaseNodeGraph>>();

    /// <summary>当前场景登记的节点图</summary>
    private List<BaseNodeGraph> currentRoomGraphs = new List<BaseNodeGraph>();

    /// <summary>
    /// 登记当前场景的所有节点图（离开房间时按图GUID导出）
    /// </summary>
    public void RegisterCurrentRoomGraphs(string sceneName)
    {
        currentRoomGraphs = new List<BaseNodeGraph>();
        foreach (var kvp in GraphCommunicator.Instance.GetAllExecutors())
        {
            BaseNodeGraph graph = kvp.Value != null ? kvp.Value.GetNodeGraph() as BaseNodeGraph : null;
            if (graph != null)
            {
                currentRoomGraphs.Add(graph);
            }
        }
        roomGraphs[sceneName] = currentRoomGraphs;
    }

    /// <summary>
    /// 导出指定房间的图变量（图GUID → 图变量数据）
    /// </summary>
    public Dictionary<string, VariableBundleData> ExportRoomGraphs(string sceneName)
    {
        Dictionary<string, VariableBundleData> result = new Dictionary<string, VariableBundleData>();
        if (roomGraphs.TryGetValue(sceneName, out List<BaseNodeGraph> graphs))
        {
            foreach (BaseNodeGraph graph in graphs)
            {
                if (graph == null) continue;
                result[graph.Guid] = graph.ExportVariables();
            }
        }
        return result;
    }

    /// <summary>
    /// 导入指定房间的图变量（存档加载时按图GUID分发）
    /// </summary>
    public void ImportRoomGraphs(string sceneName, Dictionary<string, VariableBundleData> graphData)
    {
        if (graphData == null || !roomGraphs.TryGetValue(sceneName, out List<BaseNodeGraph> graphs)) return;
        foreach (BaseNodeGraph graph in graphs)
        {
            if (graph == null) continue;
            if (graphData.TryGetValue(graph.Guid, out VariableBundleData data))
            {
                graph.ImportVariables(data);
            }
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
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 初始场景不会触发 sceneLoaded，需要手动加载一次当前场景
        LoadRoomVariableObject(SceneManager.GetActiveScene());
        RegisterCurrentRoomGraphs(SceneManager.GetActiveScene().name);
    }

    void Start()
    {
        // Awake 阶段执行器可能还没注册完，Start 阶段确保当前房间的图已登记
        RegisterCurrentRoomGraphs(SceneManager.GetActiveScene().name);
    }

    protected override void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDisable();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadRoomVariableObject(scene);
        RegisterCurrentRoomGraphs(scene.name);
        // 进入房间后应用该房间的存档（只读 archive）
        SaveSystem.ApplyRoomSave(scene.name);
    }

    /// <summary>
    /// 按场景名从 Resources 加载房间变量对象
    /// 找不到时：编辑器模式下自动在该目录创建真实资产；构建运行时退回运行时实例
    /// </summary>
    private void LoadRoomVariableObject(Scene scene)
    {
        string path = RoomVariableFolder + scene.name;
        VariableBundleObject obj = Resources.Load<VariableBundleObject>(path);
        if (obj == null)
        {
            obj = CreateRoomVariableObject(scene.name);
        }
        SetVariableObject(obj);
    }

    /// <summary>
    /// 创建房间变量对象：优先在 Resources 目录下创建真实资产，否则退回运行时实例
    /// </summary>
    private VariableBundleObject CreateRoomVariableObject(string sceneName)
    {
#if UNITY_EDITOR
        VariableBundleObject asset = ScriptableObject.CreateInstance<VariableBundleObject>();
        string assetPath = "Assets/Resources/" + RoomVariableFolder + sceneName + ".asset";

        // 目录不存在时先创建
        string directory = System.IO.Path.GetDirectoryName(assetPath);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"房间变量：场景 '{sceneName}' 未找到变量对象，已自动创建资产 '{assetPath}'");
        return asset;
#else
        Debug.LogWarning($"房间变量：Resources 中未找到场景 '{sceneName}' 对应的变量对象（路径: {RoomVariableFolder}{sceneName}），使用运行时实例");
        return ScriptableObject.CreateInstance<VariableBundleObject>();
#endif
    }
}
