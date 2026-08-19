using UnityEngine;

/// <summary>
/// 游戏全局变量管理器（单例）- 持久化全局变量
/// 使用手动拖入的变量对象：优先使用当前变量对象，未填入时使用默认变量对象
/// </summary>
[SceneAutoCreate]
public class GameGlobalVariableManager : PersistentVariableManager
{
    /// <summary>默认全局变量对象（当前变量对象未填入时使用）</summary>
    [SerializeField] private VariableBundleObject defaultVariableObject;

    private static GameGlobalVariableManager _instance;

    public static GameGlobalVariableManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameGlobalVariableManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(GameGlobalVariableManager).Name);
                    _instance = go.AddComponent<GameGlobalVariableManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public override PersistentVariableScope Scope => PersistentVariableScope.Global;

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
        // 未手动填入当前变量对象时，使用默认变量对象；两者都没有时直接报错
        if (!HasAssignedVariableObject)
        {
            if (defaultVariableObject == null)
            {
                Debug.LogError($"{nameof(GameGlobalVariableManager)}: 未设置默认全局变量对象（Default Variable Object）且当前变量对象为空，请手动拖入 VariableBundleObject");
                return;
            }
            SetVariableObject(defaultVariableObject);
        }
    }

    /// <summary>
    /// 全局变量不允许静默使用运行时实例，触发隐式创建时同样报错
    /// </summary>
    protected override VariableBundleObject CreateVariableObject()
    {
        Debug.LogError($"{nameof(GameGlobalVariableManager)}: 未配置全局变量对象，请手动拖入 VariableBundleObject");
        return base.CreateVariableObject();
    }
}
