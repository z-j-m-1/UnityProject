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
    }

    protected override void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDisable();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadRoomVariableObject(scene);
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
