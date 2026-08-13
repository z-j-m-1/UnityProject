using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

// 通用节点图执行器 - 挂载到GameObject上使用
public class GraphExecutor : MonoBehaviour
{
    [SerializeField] private BaseNodeGraph nodeGraph;
    [SerializeField] private bool autoExecute = false;
    [SerializeField] private float executeInterval = 1.0f;
    [SerializeField] private int executeCount = 0; // 0 代表无限执行

    private Coroutine executeCoroutine;
    private int currentExecuteCount = 0;

    void Start()
    {
        if (nodeGraph == null)
        {
            Debug.LogWarning("节点图为空");
            return;
        }

        nodeGraph.SetAttachedObject(gameObject);

        if (autoExecute)
        {
            Execute();
        }

        GraphCommunicator.Instance.RegisterGraphExecutor(this.gameObject);
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

    // 执行协程
    private IEnumerator ExecuteCoroutine()
    {
        if (nodeGraph == null)
        {
            Debug.LogWarning("节点图为空");
            yield break;
        }

        if (nodeGraph.startNode == null)
        {
            Debug.LogWarning("没有StartNode");
            yield break;
        }

        // 重置执行次数
        currentExecuteCount = 0;

        while (true)
        {
            yield return new WaitForSeconds(executeInterval);

            // 执行节点链
            nodeGraph.CurrentNode = nodeGraph.startNode;
            int maxLoop = 100;
            int counter = 0;

            while (nodeGraph.CurrentNode != null && counter < maxLoop)
            {
                nodeGraph.CurrentNode.Execute();
                nodeGraph.CurrentNode = nodeGraph.CurrentNode.GetConnectedNode();
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