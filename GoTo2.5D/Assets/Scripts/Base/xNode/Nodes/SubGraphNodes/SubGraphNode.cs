using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 子图执行节点：调用另一张节点图作为子流程。
/// - 动态端口自动与子图「参数输入/参数输出」节点同步（端口名 = 参数名）；
/// - 执行时：父图连线值 → 子图变量 → 跑子图链 → 输出节点输入求值 → 输出端口；
/// - 嵌套有深度上限（防循环引用）。
/// </summary>
[CreateNodeMenu("子图/执行")]
[NodeTint("#8844CC")]
[NodeWidth(320)]
public class SubGraphNode : FlowNode
{
    [Header("子图资产")]
    public BaseNodeGraph subGraph;

    [Header("入口标识（空 = 子图默认起点）")]
    public string entryIdentifier;

    [Header("调用时重置子图变量")]
    public bool resetVariablesOnCall = false;

    private const int MaxDepth = 8;
    private static int depth;

    /// <summary>输出参数缓存：子图链跑完后由子图变量回填</summary>
    private readonly Dictionary<string, object> outputCache = new Dictionary<string, object>();

    protected override void Init()
    {
        base.Init();
        SyncParameterPorts();
    }

    private void OnValidate()
    {
        SyncParameterPorts();
    }

    public override IEnumerator GetFlow()
    {
        if (subGraph == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未指定子图资产");
            yield break;
        }

        BaseNode start = ResolveStart(subGraph);
        if (start == null)
        {
            NodeLog.Error($"{GetType().Name}: 子图 '{subGraph.name}' 找不到起点（入口 '{entryIdentifier}'）");
            yield break;
        }

        if (depth >= MaxDepth)
        {
            NodeLog.Error($"{GetType().Name}: 子图嵌套超过 {MaxDepth} 层，疑似循环引用，已中止");
            yield break;
        }

        depth++;
        try
        {
            if (resetVariablesOnCall)
            {
                subGraph.ResetGraph();
            }

            // 父图连线值注入子图变量
            InjectInputs();

            // 跑子图链（yield 其内部等待/流程；上下文保持父执行器）
            yield return GraphChainRunner.RunChain(subGraph, start, NodeExecuteContext.Current, null);

            // 子图变量回读到输出端口缓存
            ReadBackOutputs();
        }
        finally
        {
            depth--;
        }
    }

    /// <summary>输出动态端口求值：返回子图链跑完后的缓存值（未执行过返回默认）</summary>
    public override object GetValue(NodePort port)
    {
        if (port != null && port.IsOutput)
        {
            if (outputCache.TryGetValue(port.fieldName, out object value))
            {
                return value;
            }
        }
        return null;
    }

    private BaseNode ResolveStart(BaseNodeGraph graph)
    {
        if (!string.IsNullOrEmpty(entryIdentifier))
        {
            EntryNode entry = graph.GetEntryNode(entryIdentifier);
            if (entry != null) return entry;
        }
        return graph.startNode;
    }

    /// <summary>父图连线值注入子图统一调用参数存储（图内「参数/输入」节点读取）</summary>
    private void InjectInputs()
    {
        foreach (NodePort port in DynamicInputs)
        {
            if (!port.IsConnected) continue;
            subGraph.SetInvocationParam(port.fieldName, port.GetInputValue());
        }
    }

    private void ReadBackOutputs()
    {
        outputCache.Clear();
        foreach (NodePort port in DynamicOutputs)
        {
            if (!port.IsConnected) continue;
            SubGraphOutputNodeBase outNode = FindOutputNode(port.fieldName);
            if (outNode == null) continue;
            outputCache[port.fieldName] = outNode.EvaluateValue();
        }
    }

    private SubGraphOutputNodeBase FindOutputNode(string parameterName)
    {
        if (subGraph == null) return null;
        foreach (XNode.Node node in subGraph.nodes)
        {
            if (node is SubGraphOutputNodeBase output && output.parameterName == parameterName)
            {
                return output;
            }
        }
        return null;
    }

    /// <summary>
    /// 幂等同步动态端口：期望集合 = 子图全部参数节点（端口名 = 参数名）；
    /// 删除多余/类型不符端口，补齐缺失端口；同名去重（先输入后输出）。
    /// </summary>
    private void SyncParameterPorts()
    {
        List<(string name, System.Type type)> expectIn = new List<(string, System.Type)>();
        List<(string name, System.Type type)> expectOut = new List<(string, System.Type)>();

        if (subGraph != null)
        {
            foreach (XNode.Node node in subGraph.nodes)
            {
                if (node is SubGraphInputNodeBase input && !string.IsNullOrEmpty(input.parameterName))
                {
                    expectIn.Add((input.parameterName, input.ParamType));
                }
                else if (node is SubGraphOutputNodeBase output && !string.IsNullOrEmpty(output.parameterName))
                {
                    expectOut.Add((output.parameterName, output.ParamType));
                }
            }
            expectIn.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            expectOut.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        }

        // 删除不再存在或类型不符的动态端口
        List<NodePort> toRemove = new List<NodePort>();
        foreach (NodePort port in DynamicPorts)
        {
            bool matched = false;
            foreach (var e in expectIn)
            {
                if (e.name == port.fieldName && e.type == port.ValueType && port.IsInput) { matched = true; break; }
            }
            if (!matched)
            {
                foreach (var e in expectOut)
                {
                    if (e.name == port.fieldName && e.type == port.ValueType && port.IsOutput) { matched = true; break; }
                }
            }
            if (!matched) toRemove.Add(port);
        }
        foreach (NodePort port in toRemove)
        {
            RemoveDynamicPort(port);
        }

        // 补齐缺失端口（同名去重）
        List<string> used = new List<string>();
        foreach (var e in expectIn)
        {
            if (used.Contains(e.name)) continue;
            used.Add(e.name);
            if (GetPort(e.name) == null)
            {
                AddDynamicInput(e.type, ConnectionType.Override, TypeConstraint.Strict, e.name);
            }
        }
        foreach (var e in expectOut)
        {
            if (used.Contains(e.name)) continue;
            used.Add(e.name);
            if (GetPort(e.name) == null)
            {
                AddDynamicOutput(e.type, ConnectionType.Override, TypeConstraint.Strict, e.name);
            }
        }
    }
}

