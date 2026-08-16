using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 图状态机：每个状态 = 一张子图；切换 = 停当前链 + 起新链（复用 GraphChainRunner）。
/// - 目标解析：链执行时宿主 = 状态机自身（Attached 目标 = 状态机物体）；
/// - 事件驱动可选：subscribeEntries 开启时订阅当前状态子图的入口事件（命中即从该入口重跑当前状态）；
/// - 切换入口：C# / UnityEvent 调 TransitionTo(name)，或图内用「状态机/切换」节点。
/// </summary>
public class GraphStateMachine : MonoBehaviour
{
    [Serializable]
    public class GraphState
    {
        [Header("状态名")]
        public string stateName;

        [Header("状态子图")]
        public BaseNodeGraph graph;

        [Header("入口标识（空 = 子图默认起点）")]
        public string entryIdentifier;

        [Header("链跑完后是否循环")]
        public bool loop = false;
    }

    [Header("状态列表")]
    public List<GraphState> states = new List<GraphState>();

    [Header("初始状态名（空 = 不自动进入）")]
    public string initialState;

    [Header("订阅当前状态子图入口事件")]
    public bool subscribeEntries = false;

    /// <summary>当前状态名（只读）</summary>
    public string CurrentState { get; private set; }

    private GraphState current;
    private Coroutine chainCoroutine;
    private Action<GraphEvent> handler;

    private void OnEnable()
    {
        handler = OnGraphEvent;
        GraphEvent.Subscribe(handler);
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(initialState))
        {
            TransitionTo(initialState);
        }
    }

    private void OnDisable()
    {
        if (handler != null)
        {
            GraphEvent.Unsubscribe(handler);
            handler = null;
        }
        StopChain();
    }

    /// <summary>切换到指定状态（停止当前链，启动新状态链）</summary>
    public void TransitionTo(string stateName)
    {
        if (string.IsNullOrEmpty(stateName)) return;

        GraphState next = states.Find(s => s.stateName == stateName);
        if (next == null || next.graph == null)
        {
            NodeLog.Warning($"GraphStateMachine '{name}': 找不到状态 '{stateName}'（或子图为空）");
            return;
        }

        StopChain();
        current = next;
        CurrentState = stateName;

        BaseNode start = ResolveStart(next.graph, next.entryIdentifier);
        if (start != null)
        {
            chainCoroutine = StartCoroutine(RunState(next, start));
        }
        NodeLog.Info($"GraphStateMachine '{name}': 切换到状态 '{stateName}'");
    }

    /// <summary>从当前状态图的某个入口重跑一条链（替换当前链）</summary>
    public void ExecuteFromCurrent(EntryNode entry)
    {
        if (current == null || entry == null || current.graph == null) return;
        StopChain();
        chainCoroutine = StartCoroutine(RunState(current, entry));
    }

    private IEnumerator RunState(GraphState state, BaseNode start)
    {
        do
        {
            yield return GraphChainRunner.RunChain(state.graph, start, this, null);
            if (state.loop)
            {
                yield return null;
            }
        } while (state.loop && current == state);
    }

    private void StopChain()
    {
        if (chainCoroutine != null)
        {
            StopCoroutine(chainCoroutine);
            chainCoroutine = null;
        }
    }

    private BaseNode ResolveStart(BaseNodeGraph graph, string entryIdentifier)
    {
        if (!string.IsNullOrEmpty(entryIdentifier))
        {
            EntryNode entry = graph.GetEntryNode(entryIdentifier);
            if (entry != null) return entry;
        }
        return graph.startNode;
    }

    private void OnGraphEvent(GraphEvent evt)
    {
        if (!subscribeEntries || current == null || current.graph == null) return;
        EntryNode entry = current.graph.GetEntryNode(evt.eventId);
        if (entry == null) return;
        ExecuteFromCurrent(entry);
    }
}