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

// 通用节点图执行器 - 挂载到GameObject上使用
public class GraphExecutor : MonoBehaviour
{
    [SerializeField] private BaseNodeGraph nodeGraph;
    [SerializeField] private bool autoExecute = false;
    [SerializeField] private float executeInterval = 1.0f;
    [SerializeField] private int executeCount = 0; // 0 代表无限执行
    [SerializeField] private GraphExecutionMode executionMode = GraphExecutionMode.Default;
    [SerializeField] private string entryIdentifier;

    private Coroutine executeCoroutine;
    private int currentExecuteCount = 0;

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
        if (executeCoroutine != null)
        {
            StopCoroutine(executeCoroutine);
        }
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
            yield break;
        }

        // 重置执行次数
        currentExecuteCount = 0;

        while (true)
        {
            yield return new WaitForSeconds(executeInterval);

            // 执行节点链
            nodeGraph.CurrentNode = startNode;
            int maxLoop = 100;
            int counter = 0;

            while (nodeGraph.CurrentNode != null && counter < maxLoop)
            {
                BaseNode node = nodeGraph.CurrentNode;
                node.Execute();

                // 节点可返回协程流程（等待/等待条件等），执行器 yield 暂停链直到完成
                IEnumerator flow = node.GetFlow();
                if (flow != null)
                {
                    yield return flow;
                }

                nodeGraph.CurrentNode = node.GetConnectedNode();
                counter++;
            }

            if (counter >= maxLoop)
                Debug.LogWarning("执行达到最大循环次数");

            currentExecuteCount++;

            // 检查是否达到执行次数限制
            if (executeCount > 0 && currentExecuteCount >= executeCount)
            {
                Debug.Log($"节点图 '{gameObject.name}' 已执行 {executeCount} 次，自动停止");
                executeCoroutine = null;
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