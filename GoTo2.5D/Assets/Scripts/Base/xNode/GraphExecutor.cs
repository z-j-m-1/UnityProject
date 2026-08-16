using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>图执行模式：默认（startNode）/ 入口（按标识符或 GUID 查找入口节点）</summary>
public enum GraphExecutionMode
{
    Default,
    Entry
}

/// <summary>图执行触发策略：执行中再次被触发时的行为</summary>
public enum GraphExecutionTriggerPolicy
{
    Restart,               // 停止当前并整条链重跑（默认）
    IgnoreWhileRunning,    // 运行中忽略重复触发
    Queue                  // 运行中排队，当前跑完后自动再跑一轮
}

// 通用节点图执行器 - 挂载到GameObject上使用
public class GraphExecutor : MonoBehaviour
{
    [SerializeField] private BaseNodeGraph nodeGraph;
    [SerializeField] private bool autoExecute = false;
    [SerializeField] private float executeInterval = 1.0f;
    [SerializeField] private int executeCount = 0; // 0 代表无限执行
    [SerializeField] private GraphExecutionMode executionMode = GraphExecutionMode.Default;
    [SerializeField] private string entryIdentifier;
    [SerializeField] private GraphExecutionTriggerPolicy triggerPolicy = GraphExecutionTriggerPolicy.Restart;

    private Coroutine executeCoroutine;
    private int currentExecuteCount = 0;
    private bool queueTriggered;

    void Awake()
    {
        if (nodeGraph == null)
        {
            Debug.LogWarning("节点图为空");
            return;
        }

        nodeGraph.SetAttachedObject(gameObject);
        GraphCommunicator.Instance.RegisterGraphExecutor(this.gameObject);
    }
    void Start()
    {
        if (nodeGraph == null)
        {
            Debug.LogWarning("节点图为空");
            return;
        }

        if (autoExecute)
        {
            Execute();
        }

    }

    void OnDestroy()
    {
        if (executeCoroutine != null)
        {
            StopCoroutine(executeCoroutine);
            executeCoroutine = null;
        }
    }

    // 执行节点图（启动协程）
    public void Execute()
    {
        switch (triggerPolicy)
        {
            case GraphExecutionTriggerPolicy.IgnoreWhileRunning:
                if (executeCoroutine != null)
                {
                    NodeLog.Info($"GraphExecutor '{gameObject.name}': 正在执行中，忽略本次触发");
                    return;
                }
                break;

            case GraphExecutionTriggerPolicy.Queue:
                if (executeCoroutine != null)
                {
                    queueTriggered = true;
                    NodeLog.Info($"GraphExecutor '{gameObject.name}': 正在执行中，已排队一次触发");
                    return;
                }
                break;

            case GraphExecutionTriggerPolicy.Restart:
            default:
                if (executeCoroutine != null)
                {
                    StopCoroutine(executeCoroutine);
                    executeCoroutine = null;
                }
                break;
        }

        queueTriggered = false;
        executeCoroutine = StartCoroutine(ExecuteCoroutine());
    }

    /// <summary>
    /// 解析执行起点：默认执行返回 startNode；入口执行按标识符/GUID 查找入口节点
    /// 入口未找到时 LogError 并返回 null（不执行、不回退 startNode）
    /// </summary>
    private BaseNode GetStartNode()
    {
        if (nodeGraph == null) return null;

        if (executionMode == GraphExecutionMode.Entry)
        {
            EntryNode entry = nodeGraph.GetEntryNode(entryIdentifier);
            if (entry == null)
            {
                Debug.LogError($"GraphExecutor '{gameObject.name}': 入口节点未找到（标识符/GUID: '{entryIdentifier}'），不执行");
                return null;
            }
            return entry;
        }

        return nodeGraph.startNode;
    }

    // 执行协程
    private IEnumerator ExecuteCoroutine()
    {
        if (nodeGraph == null)
        {
            Debug.LogWarning("节点图为空");
            executeCoroutine = null;
            yield break;
        }

        // 解析执行起点（默认执行 = startNode；入口执行 = 按标识符/GUID 找入口节点）
        BaseNode startNode = GetStartNode();
        if (startNode == null)
        {
            if (executionMode == GraphExecutionMode.Default)
            {
                Debug.LogWarning("没有StartNode");
            }
            executeCoroutine = null;
            yield break;
        }

        // 重置执行次数
        currentExecuteCount = 0;

        while (true)
        {
            yield return new WaitForSeconds(executeInterval);

            // 执行节点链（游标为执行器私有：多个执行器跑同一张图互不干扰）
            BaseNode node = startNode;
            int maxLoop = 100;
            int counter = 0;

            while (node != null && counter < maxLoop)
            {
                node.Execute();

                // 节点可返回协程流程（等待/等待条件等），执行器 yield 暂停链直到完成
                IEnumerator flow = node.GetFlow();
                if (flow != null)
                {
                    yield return flow;
                }

                node = node.GetConnectedNode();
                counter++;
            }

            if (counter >= maxLoop)
                Debug.LogWarning("执行达到最大循环次数");

            currentExecuteCount++;

            // 检查是否达到执行次数限制
            if (executeCount > 0 && currentExecuteCount >= executeCount)
            {
                NodeLog.Info($"节点图 '{gameObject.name}' 已执行 {executeCount} 次，自动停止");
                executeCoroutine = null;

                // 排队触发：当前跑完后自动再跑一轮
                if (queueTriggered)
                {
                    queueTriggered = false;
                    Execute();
                }
                yield break;
            }
        }
    }

    public BaseNodeGraph GetNodeGraph()
    {
        return nodeGraph;
    }
        [ContextMenu("执行节点图")]
    public void ExecuteFromContextMenu()
    {
        if (nodeGraph != null)
        {
            nodeGraph.SetAttachedObject(gameObject);
            Execute();
        }
    }
}