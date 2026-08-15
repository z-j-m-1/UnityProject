using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GraphCommunicator : MonoBehaviour
{
    [SerializeField] private Dictionary<string, GraphExecutor> graphExecutors = new Dictionary<string, GraphExecutor>();

    // 单例实例
    private static GraphCommunicator _instance;
    public static GraphCommunicator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GraphCommunicator>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(GraphCommunicator).Name);
                    _instance = go.AddComponent<GraphCommunicator>();
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

        // 订阅场景卸载事件
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Start()
    {
        // 订阅通讯执行节点图事件
        ComExecutionGraphEvent.Subscribe(OnComExecutionGraph);

        // 订阅通讯设置变量事件
        ComSetVariableEvent<string>.Subscribe(OnComSetVariable<string>);
        ComSetVariableEvent<bool>.Subscribe(OnComSetVariable<bool>);
        ComSetVariableEvent<int>.Subscribe(OnComSetVariable<int>);
        ComSetVariableEvent<float>.Subscribe(OnComSetVariable<float>);


        // 订阅通讯获取变量事件
        ComGetVariableEvent<string>.Subscribe(OnComGetVariable<string>);
        ComGetVariableEvent<bool>.Subscribe(OnComGetVariable<bool>);
        ComGetVariableEvent<int>.Subscribe(OnComGetVariable<int>);
        ComGetVariableEvent<float>.Subscribe(OnComGetVariable<float>);

        // 启动时加载存档（场景变量与图执行器就绪后）
        SaveSystem.Load();

    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }

        // 取消订阅事件，避免内存泄漏
        SceneManager.sceneUnloaded -= OnSceneUnloaded;

        // 取消订阅所有通讯事件
        ComExecutionGraphEvent.Unsubscribe(OnComExecutionGraph);
        ComSetVariableEvent<string>.Unsubscribe(OnComSetVariable<string>);
        ComSetVariableEvent<bool>.Unsubscribe(OnComSetVariable<bool>);
        ComSetVariableEvent<int>.Unsubscribe(OnComSetVariable<int>);
        ComSetVariableEvent<float>.Unsubscribe(OnComSetVariable<float>);
        ComGetVariableEvent<string>.Unsubscribe(OnComGetVariable<string>);
        ComGetVariableEvent<bool>.Unsubscribe(OnComGetVariable<bool>);
        ComGetVariableEvent<int>.Unsubscribe(OnComGetVariable<int>);
        ComGetVariableEvent<float>.Unsubscribe(OnComGetVariable<float>);

    }

    // 场景卸载时清空字典
    private void OnSceneUnloaded(Scene scene)
    {
        ClearAllExecutors();
        Debug.Log($"场景 '{scene.name}' 已卸载，GraphExecutors 字典已清空");
    }

    // 注册 GraphExecutor（使用物体名称作为键）
    public void RegisterGraphExecutor(GameObject gameObject)
    {
        if (gameObject == null) return;

        GraphExecutor executor = gameObject.GetComponent<GraphExecutor>();
        if (executor != null && !graphExecutors.ContainsKey(gameObject.name))
        {
            graphExecutors.Add(gameObject.name, executor);
            Debug.Log($"已注册 GraphExecutor: {gameObject.name}");
        }
    }

    // 通过物体名称获取 GraphExecutor
    public GraphExecutor GetGraphExecutor(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)) return null;

        if (graphExecutors.TryGetValue(objectName, out GraphExecutor executor))
        {
            return executor;
        }
        return null;
    }

    // 通过物体名称检查是否已注册
    public bool IsRegistered(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)) return false;
        return graphExecutors.ContainsKey(objectName);
    }

    // 获取所有已注册的 GraphExecutor（供存档系统等读取）
    public Dictionary<string, GraphExecutor> GetAllExecutors()
    {
        return graphExecutors;
    }

    // 清空所有执行器
    public void ClearAllExecutors()
    {
        graphExecutors.Clear();
    }

    // ============ 通讯事件处理函数 ============

    /// <summary>
    /// 通讯-执行节点图事件处理
    /// </summary>
    private void OnComExecutionGraph(ComExecutionGraphEvent evt)
    {
        GraphExecutor executor = GetGraphExecutor(evt.graphName);
        if (executor != null)
        {
            executor.Execute();
            Debug.Log($"GraphCommunicator: 通讯执行节点图 '{evt.graphName}'");
        }
        else
        {
            Debug.LogError($"GraphCommunicator: 通讯执行失败，未找到名为 '{evt.graphName}' 的 GraphExecutor");
        }
    }

    // ============ 通讯设置变量事件处理 ============

    private void OnComSetVariable<T>(ComSetVariableEvent<T> evt)
    {
        SetVariableInternal<T>(evt.targetName, evt.variableName, evt.guid, evt.variableValue, evt.onResolved);
    }


    /// <summary>
    /// 内部设置变量方法（名字优先 + GUID 兜底）
    /// </summary>
    private void SetVariableInternal<T>(string graphName, string varName, string guid, T varValue, System.Action<string, string> onResolved)
    {
        GraphExecutor executor = GetGraphExecutor(graphName);
        if (executor != null)
        {
            BaseNodeGraph graph = executor.GetNodeGraph() as BaseNodeGraph;
            if (graph != null)
            {
                if (graph.TrySetVariable(varName, guid, varValue, out string actualName, out string actualGuid))
                {
                    onResolved?.Invoke(actualName, actualGuid);
                    Debug.Log($"GraphCommunicator: 通讯设置变量 '{graphName}.{actualName}' = '{varValue}' (类型: {typeof(T).Name})");
                }
                else
                {
                    // 名字和GUID都找不到：按名字直接创建/设置
                    graph.Set(varName, varValue);
                    onResolved?.Invoke(varName, guid);
                    Debug.Log($"GraphCommunicator: 通讯设置变量 '{graphName}.{varName}' = '{varValue}' (类型: {typeof(T).Name})");
                }
            }
            else
            {
                Debug.LogError($"GraphCommunicator: 通讯设置变量失败，目标 '{graphName}' 的节点图不是 BaseNodeGraph 类型");
            }
        }
        else
        {
            Debug.LogError($"GraphCommunicator: 通讯设置变量失败，未找到名为 '{graphName}' 的 GraphExecutor");
        }
    }

    // ============ 通讯获取变量事件处理 ============

    private void OnComGetVariable<T>(ComGetVariableEvent<T> evt)
    {
        GetVariableInternal<T>(evt.targetName, evt.variableName, evt.guid, evt.defaultValue, evt.callback);
    }

    /// <summary>
    /// 内部获取变量方法（名字优先 + GUID 兜底）
    /// </summary>
    private void GetVariableInternal<T>(string graphName, string varName, string guid, T defaultValue, System.Action<T, string, string> callback)
    {
        GraphExecutor executor = GetGraphExecutor(graphName);
        if (executor != null)
        {
            BaseNodeGraph graph = executor.GetNodeGraph() as BaseNodeGraph;
            if (graph != null)
            {
                if (graph.TryGetVariable(varName, guid, out T value, out string actualName, out string actualGuid))
                {
                    callback?.Invoke(value, actualName, actualGuid);
                    Debug.Log($"GraphCommunicator: 通讯获取变量 '{graphName}.{actualName}' = '{value}' (类型: {typeof(T).Name})");
                    return;
                }
                Debug.LogError($"GraphCommunicator: 通讯获取变量失败，变量 '{graphName}.{varName}' 不存在");
            }
            else
            {
                Debug.LogError($"GraphCommunicator: 通讯获取变量失败，目标 '{graphName}' 的节点图不是 BaseNodeGraph 类型");
            }
        }
        else
        {
            Debug.LogError($"GraphCommunicator: 通讯获取变量失败，未找到名为 '{graphName}' 的 GraphExecutor");
        }

        // 如果失败，返回默认值
        callback?.Invoke(defaultValue, varName, guid);
    }
}