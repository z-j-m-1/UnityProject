using UnityEngine;

/// <summary>
/// 游戏全局变量管理器（单例）- 持久化全局变量
/// </summary>
public class GameGlobalVariableManager : PersistentVariableManager
{
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
}
