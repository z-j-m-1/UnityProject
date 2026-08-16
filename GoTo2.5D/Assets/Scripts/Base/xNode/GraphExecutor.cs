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
    [SerializeField] private bool subscribeEntryEvent;

    private Coroutine executeCoroutine;
    private int currentExecuteCount = 0;
    private bool queueTriggered;
    private BaseNode pendingStartOverride;
    private System.Action<GraphEvent> entryEventHandler;

    /// <summary>当前正在执行的节点（供编辑器运行高亮；未执行时为 null）</summary>
    [System.NonSerialized] private BaseNode currentNode;

    /// <summary>当前正在执行的节点（编辑器运行高亮用）</summary>
    public BaseNode RunningNode => currentNode;

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

        if (subscribeEntryEvent)
        {
            SubscribeEntryEvent();
        }
    }

    void OnDestroy()
    {
        if (executeCoroutine != null)
        {
            StopCoroutine(executeCoroutine);
            executeCoroutine = null;
            currentNode = null;
        }

        if (entryEventHandler != null)
        {
            GraphEvent.Unsubscribe(entryEventHandler);
            entryEventHandler = null;
        }
    }

    // 执行节点图（启动协程，默认起点）
    public void Execute() => ExecuteFrom(null);

    /// <summary>从指定节点开始执行（null = 按配置的默认/入口起点）；触发策略同样生效</summary>
    public void ExecuteFrom(BaseNode startOverride)
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
                    currentNode = null;
                }
                break;
        }

        queueTriggered = false;
        pendingStartOverride = startOverride;
        executeCoroutine = StartCoroutine(ExecuteCoroutine());
    }

    /// <summary>订阅入口事件：入口模式 + subscribeEntryEvent 开启时，事件触发从该入口执行</summary>
    private void SubscribeEntryEvent()
    {
        if (executionMode != GraphExecutionMode.Entry || string.IsNullOrEmpty(entryIdentifier)) return;

        entryEventHandler = OnGraphEvent;
        GraphEvent.Subscribe(entryEventHandler);
        NodeLog.Info($"GraphExecutor '{gameObject.name}': 已订阅入口事件 '{entryIdentifier}'");
    }

    private void OnGraphEvent(GraphEvent evt)
    {
        if (evt.eventId != entryIdentifier) return;

        EntryNode entry = nodeGraph != null ? nodeGraph.GetEntryNode(entryIdentifier) : null;
        if (entry == null)
        {
            NodeLog.Warning($"GraphExecutor '{gameObject.name}': 入口事件 '{entryIdentifier}' 未找到对应入口节点");
            return;
        }
        ExecuteFrom(entry);
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
            currentNode = null;
            yield break;
        }

        // 解析执行起点：指定起点（事件等）优先，否则按配置（默认 = startNode；入口 = 标识符/GUID）
        BaseNode startNode = pendingStartOverride != null ? pendingStartOverride : GetStartNode();
        pendingStartOverride = null;
        if (startNode == null)
        {
            if (executionMode == GraphExecutionMode.Default)
            {
                Debug.LogWarning("没有StartNode");
            }
            executeCoroutine = null;
            currentNode = null;
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
                currentNode = node;

                // 执行上下文：让节点能解析到"当前执行器"的目标物体
                NodeExecuteContext.Current = this;
                try
                {
                    node.Execute();
                }
                finally
                {
                    NodeExecuteContext.Current = null;
                }

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
                currentNode = null;

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