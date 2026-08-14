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
    /// 按场景名从 Resources 加载房间变量对象，找不到时用运行时实例兜底
    /// </summary>
    private void LoadRoomVariableObject(Scene scene)
    {
        SetVariableObject(LoadVariableObject(RoomVariableFolder + scene.name));
    }
}
