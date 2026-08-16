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

/// <summary>图执行触发策略：同一起点（startNode / 入口节点）再次被触发时的行为</summary>
public enum GraphExecutionTriggerPolicy
{
    Restart,               // 停止当前链并整条重跑（默认）
    IgnoreWhileRunning,    // 运行中忽略重复触发
    Queue                  // 运行中排队，当前链跑完后自动再跑一轮
}

/// <summary>入口事件订阅模式</summary>
public enum EntryEventSubscribeMode
{
    Off,             // 不订阅事件
    CurrentEntry,    // 只订阅自己入口（entryIdentifier）的事件
    AllEntries       // 订阅图内所有入口的事件（并发执行，互不打断）
}

/// <summary>
/// 通用节点图执行器 - 挂载到 GameObject 上使用
/// 支持多链并发：每次事件 / 触发可独立启动一条链，互不打断；共享图变量（合作/独立按变量划分）
/// </summary>
public class GraphExecutor : MonoBehaviour
{
    [SerializeField] private BaseNodeGraph nodeGraph;
    [SerializeField] private bool autoExecute = false;
    [SerializeField] private float executeInterval = 1.0f;
    [SerializeField] private int executeCount = 0; // 0 代表无限执行
    [SerializeField] private GraphExecutionMode executionMode = GraphExecutionMode.Default;
    [SerializeField] private string entryIdentifier;
    [SerializeField] private GraphExecutionTriggerPolicy triggerPolicy = GraphExecutionTriggerPolicy.Restart;
    [SerializeField] private EntryEventSubscribeMode entryEventSubscribe = EntryEventSubscribeMode.Off;

    /// <summary>单条执行链的运行状态（每条链独立）</summary>
    private class RunState
    {
        public Coroutine coroutine;
        public BaseNode currentNode;
        public int executeCount;
        public bool queued;
    }

    /// <summary>运行中的链：起点节点 → 状态（支持并发多条）</summary>
    private readonly Dictionary<BaseNode, RunState> runs = new Dictionary<BaseNode, RunState>();

    private System.Action<GraphEvent> entryEventHandler;

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

        if (entryEventSubscribe != EntryEventSubscribeMode.Off)
        {
            SubscribeEntryEvent();
        }
    }

    void OnDestroy()
    {
        foreach (RunState run in runs.Values)
        {
            if (run.coroutine != null) StopCoroutine(run.coroutine);
        }
        runs.Clear();

        if (entryEventHandler != null)
        {
            GraphEvent.Unsubscribe(entryEventHandler);
            entryEventHandler = null;
        }
    }

    // ============ 执行入口 ============

    /// <summary>按配置的默认/入口起点执行（启动一条新链；同起点按触发策略处理）</summary>
    public void Execute() => ExecuteFrom(null);

    /// <summary>从指定入口节点执行（标识符/GUID），可携带外部参数（先替换图内现有外部参数再执行）</summary>
    public void ExecuteFromEntry(string entryId, GraphParams args = null)
    {
        if (nodeGraph == null)
        {
            Debug.LogWarning("节点图为空");
            return;
        }
        EntryNode entry = nodeGraph.GetEntryNode(entryId);
        if (entry == null)
        {
            Debug.LogError($"GraphExecutor '{gameObject.name}': 入口节点未找到（标识符/GUID: '{entryId}'），不执行");
            return;
        }
        ExecuteFrom(entry, args);
    }

    /// <summary>清空图内所有外部参数</summary>
    public void ClearExternalParams() => nodeGraph?.ClearExternalParams();

    /// <summary>从指定节点开始执行一条链（null = 按配置起点）；不同起点并发执行，同一起点按触发策略处理</summary>
    public void ExecuteFrom(BaseNode start) => ExecuteFrom(start, null);

    /// <summary>从指定节点开始执行一条链，args 非空时先替换图内外部参数再执行（触发策略同无参版本）</summary>
    public void ExecuteFrom(BaseNode start, GraphParams args)
    {
        if (args != null && args.Count > 0 && nodeGraph != null)
        {
            nodeGraph.ClearExternalParams();
            foreach (var kv in args.Data)
            {
                nodeGraph.SetExternalParam(kv.Key, kv.Value);
            }
            NodeLog.Info($"GraphExecutor '{gameObject.name}': 已注入外部参数 {args.Count} 个");
        }

        if (start == null)
        {
            start = GetStartNode();
            if (start == null)
            {
                if (executionMode == GraphExecutionMode.Default)
                {
                    Debug.LogWarning("没有StartNode");
                }
                return;
            }
        }

        if (runs.TryGetValue(start, out RunState run))
        {
            switch (triggerPolicy)
            {
                case GraphExecutionTriggerPolicy.IgnoreWhileRunning:
                    NodeLog.Info($"GraphExecutor '{gameObject.name}': 起点 '{start.name}' 正在执行中，忽略本次触发");
                    return;

                case GraphExecutionTriggerPolicy.Queue:
                    run.queued = true;
                    NodeLog.Info($"GraphExecutor '{gameObject.name}': 起点 '{start.name}' 正在执行中，已排队一次触发");
                    return;

                case GraphExecutionTriggerPolicy.Restart:
                default:
                    StopCoroutine(run.coroutine);
                    runs.Remove(start);
                    run = null;
                    break;
            }
        }

        if (run == null)
        {
            run = new RunState();
            runs[start] = run;
        }
        run.queued = false;
        run.executeCount = 0;
        run.coroutine = StartCoroutine(ExecuteChain(start, run));
    }

    /// <summary>
    /// 解析执行起点（默认执行 = startNode；入口执行 = 按标识符/GUID 找入口节点）
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

    // ============ 单条链的协程 ============

    private IEnumerator ExecuteChain(BaseNode start, RunState run)
    {
        while (true)
        {
            yield return new WaitForSeconds(executeInterval);

            // 共享链执行器：走链 + yield 流程（含"当前节点"回调与循环上限保护）
            yield return GraphChainRunner.RunChain(nodeGraph, start, this, n => run.currentNode = n);

            run.executeCount++;
            run.currentNode = null;

            // 检查是否达到执行次数限制
            if (executeCount > 0 && run.executeCount >= executeCount)
            {
                NodeLog.Info($"图 '{gameObject.name}' 起点 '{start.name}' 已执行 {executeCount} 次，自动停止");
                runs.Remove(start);

                if (run.queued)
                {
                    ExecuteFrom(start);
                }
                yield break;
            }
        }
    }

    // ============ 入口事件订阅 ============

    private void SubscribeEntryEvent()
    {
        if (entryEventSubscribe == EntryEventSubscribeMode.Off) return;

        if (entryEventSubscribe == EntryEventSubscribeMode.CurrentEntry
            && (executionMode != GraphExecutionMode.Entry || string.IsNullOrEmpty(entryIdentifier)))
        {
            return;
        }

        entryEventHandler = OnGraphEvent;
        GraphEvent.Subscribe(entryEventHandler);
        NodeLog.Info($"GraphExecutor '{gameObject.name}': 已订阅入口事件（{entryEventSubscribe}）");
    }

    private void OnGraphEvent(GraphEvent evt)
    {
        EntryNode entry;
        if (entryEventSubscribe == EntryEventSubscribeMode.CurrentEntry)
        {
            if (evt.eventId != entryIdentifier) return;
            entry = nodeGraph != null ? nodeGraph.GetEntryNode(entryIdentifier) : null;
        }
        else
        {
            // AllEntries：按事件标识解析对应入口
            entry = nodeGraph != null ? nodeGraph.GetEntryNode(evt.eventId) : null;
        }

        if (entry == null)
        {
            NodeLog.Warning($"GraphExecutor '{gameObject.name}': 事件 '{evt.eventId}' 未找到对应入口节点");
            return;
        }
        ExecuteFrom(entry, evt.data);
    }

    // ============ 运行状态（编辑器高亮等） ============

    /// <summary>所有运行中链的当前执行节点（编辑器运行高亮用，多链可多个）</summary>
    public IEnumerable<BaseNode> RunningNodes
    {
        get
        {
            foreach (RunState run in runs.Values)
            {
                if (run.currentNode != null)
                {
                    yield return run.currentNode;
                }
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
