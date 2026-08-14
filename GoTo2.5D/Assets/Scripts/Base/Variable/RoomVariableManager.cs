using UnityEngine;

/// <summary>
/// 房间变量管理器（单例）- 持久化房间变量
/// </summary>
public class RoomVariableManager : PersistentVariableManager
{
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
}
