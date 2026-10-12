using UnityEngine;

/// <summary>
/// 游戏全局变量管理器（单例）- 持久化全局变量
///
/// 变量对象来源优先级：
///   ① Inspector 手动拖入的「当前变量对象」；
///   ② Inspector 手动拖入的「默认全局变量对象」；
///   ③ **自动**到 Resources/PersistentVariables/Global 下找资产 ——
///      优先用名为「默认公共变量」的，找不到就用该目录下其它 VariableBundleObject（按名字排序取第一个，结果稳定）；
///   ④ 以上都没有，才报错。
///
/// 注意全局变量必须是**可持续化资产**，所以自动查找失败时不会退化成"只在运行时存在的临时实例"。
/// </summary>
[SceneAutoCreate]
public class GameGlobalVariableManager : PersistentVariableManager
{
    /// <summary>全局变量资产所在的 Resources 相对路径（不带 Assets/Resources/ 前缀与扩展名）</summary>
    public const string GlobalVariableResourcePath = "PersistentVariables/Global";

    /// <summary>该目录下优先使用的资产名（= 文件名去掉 .asset）</summary>
    public const string PreferredVariableAssetName = "默认公共变量";

    /// <summary>默认全局变量对象（当前变量对象未填入时使用；留空则先尝试自动从 Resources 加载）</summary>
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

        // 已手动填入「当前变量对象」→ 直接用它，不需要默认对象
        if (HasAssignedVariableObject) return;

        // 默认对象也留空 → 自动去 Resources 目录找
        if (defaultVariableObject == null)
        {
            defaultVariableObject = LoadDefaultVariableObjectFromResources();
        }

        if (defaultVariableObject == null)
        {
            Debug.LogError(
                $"{nameof(GameGlobalVariableManager)}: 未设置默认全局变量对象（Default Variable Object）、当前变量对象为空，" +
                $"且 Resources/{GlobalVariableResourcePath} 下也没有找到任何 {nameof(VariableBundleObject)}。" +
                $"请手动拖入，或把资产放到 Assets/Resources/{GlobalVariableResourcePath} 下。");
            return;
        }

        SetVariableObject(defaultVariableObject);
    }

    /// <summary>
    /// 隐式取用变量对象时（例如事件早于 OnEnable 配置触发、或外部代码直接读写）也走同一套查找，
    /// 而不是创建一个"只存在于运行时、不会存档"的临时实例 —— 全局变量必须是可持续化资产。
    /// 走到这里仍然找不到才报错。
    /// </summary>
    protected override VariableBundleObject CreateVariableObject()
    {
        VariableBundleObject found = defaultVariableObject != null
            ? defaultVariableObject
            : LoadDefaultVariableObjectFromResources();

        if (found != null)
        {
            defaultVariableObject = found;
            return found;
        }

        Debug.LogError(
            $"{nameof(GameGlobalVariableManager)}: 未配置全局变量对象，" +
            $"且 Resources/{GlobalVariableResourcePath} 下没有找到 {nameof(VariableBundleObject)}。请手动拖入或补齐资产。");

        // 兜底：仍给一个临时实例，避免上层直接空引用崩掉（数据不会持久化，已经报错提示）
        return base.CreateVariableObject();
    }

    /// <summary>
    /// 到 Resources/PersistentVariables/Global 下找全局变量资产：
    ///   ① 优先名为「默认公共变量」的；
    ///   ② 否则用该目录下的其它资产 —— 按名字排序取第一个，
    ///      **刻意排序**是因为 Resources.LoadAll 的返回顺序不保证稳定，不排序会导致"每次运行用到不同资产"；
    ///   ③ 一个都没有时返回 null（由调用处报错）。
    ///
    /// 只匹配 VariableBundleObject 类型 —— 目录里其它类型的资产会被自动忽略。
    /// </summary>
    private static VariableBundleObject LoadDefaultVariableObjectFromResources()
    {
        VariableBundleObject[] all = Resources.LoadAll<VariableBundleObject>(GlobalVariableResourcePath);
        if (all == null || all.Length == 0) return null;

        foreach (VariableBundleObject candidate in all)
        {
            if (candidate != null && candidate.name == PreferredVariableAssetName)
            {
                Debug.Log(
                    $"{nameof(GameGlobalVariableManager)}: 已自动加载默认全局变量对象「{candidate.name}」" +
                    $"（Resources/{GlobalVariableResourcePath}）");
                return candidate;
            }
        }

        VariableBundleObject fallback = null;
        foreach (VariableBundleObject candidate in all)
        {
            if (candidate == null) continue;
            if (fallback == null || string.CompareOrdinal(candidate.name, fallback.name) < 0)
            {
                fallback = candidate;
            }
        }

        if (fallback != null)
        {
            Debug.LogWarning(
                $"{nameof(GameGlobalVariableManager)}: Resources/{GlobalVariableResourcePath} 下没有名为" +
                $"「{PreferredVariableAssetName}」的资产，已改用「{fallback.name}」。" +
                $"建议把要作为默认的资产命名为「{PreferredVariableAssetName}」以免将来换资产时用错。");
        }

        return fallback;
    }
}
