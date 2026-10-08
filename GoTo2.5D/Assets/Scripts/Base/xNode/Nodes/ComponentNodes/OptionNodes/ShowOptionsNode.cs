using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>
/// 选项UI-显示选项：**显示 + （可选）本节点内等待选择 + 输出选择结果**，一个节点顶原来的三个
/// （原「选项UI/等待选择」「选项UI/获取选中索引」已合并删除）。
///
/// 继承 <see cref="ConditionWaitNode"/>（而不是普通 FlowNode）是刻意的 ——
/// 这样**白拿 `timeout` + `timeoutTo` 超时分支**，可以配"选项超时自动走默认分支"。
/// ConditionWaitNode 的基类注释里写的"系统级条件（**对话选项** / 打字结束…）→ 继承本类"，指的就是这种用法；
/// <c>OnWaitStart()</c> 正好用来做"等待前的准备工作"（显示选项）。
///
/// 两种用法（互斥，与「对话UI/设置对话文本」的 waitUntilTyped + typingFinished 完全对称）：
///   1. `waitForSelection` 开（默认）→ 本节点内等选择结束，一个节点搞定，但拿不到超时分支之外的信息；
///   2. `waitForSelection` 关 → 只显示，接着把输出端口「选择完毕」接到「流程/等待条件」去等
///      （适合"显示选项后先做点别的，再等玩家选"）。
///
/// 输入 `List&lt;string&gt;` 不需要新数据类型：可直接接「参数/输入/字符串列表」或「变量操作/获取/字符串列表」。
/// </summary>
[CreateNodeMenu("选项UI/显示选项")]
[NodeTint("#000c13")]
[NodeWidth(280)]
public class ShowOptionsNode : ConditionWaitNode
{
    [Header("目标（接线：任一「取值/获取物体」节点）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject targetGameObject;

    [Header("选项文本列表（可接线；也可在此直接编辑）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public List<string> options = new List<string>();

    [Header("图标列表（可选，按索引与文本对应）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public List<Sprite> icons;

    [Header("在本节点内等待选择（关掉 = 只显示，改用下方输出端口接「流程/等待条件」）")]
    public bool waitForSelection = true;

    [Header("选择完毕（接「流程/等待条件」；fail-open：面板未配置时视为已完成）")]
    [Output]
    public bool selectionFinished;

    [Header("选中索引（-1 = 未选；接「流程分支/按索引分支」）")]
    [Output]
    public int selectedIndex = -1;

    private OptionPanelController panel;

    // 面板可能「选中后销毁自己」，销毁后 panel 会变成 Unity 假 null ——
    // 所以必须在它还活着的时候把选择结果抓下来缓存，否则 selectedIndex 会退回 -1（错误地走"没选"的分支）。
    private int resolvedIndex = -1;
    private bool hasResolved;

    /// <summary>等待开始前：解析面板并显示选项（ConditionWaitNode.Execute 会调这里）</summary>
    protected override void OnWaitStart()
    {
        // 先退订上一轮的面板（面板若已销毁，假 null 会跳过此步，退订随之失效，无副作用）
        if (panel != null) panel.SelectionMade -= OnPanelSelectionMade;

        hasResolved = false;
        resolvedIndex = -1;

        panel = NodeTargetResolver.Resolve<OptionPanelController>(this);
        if (panel == null)
        {
            // 严重：没解析到面板 → 选项不会显示；而 CheckCondition 会 fail-open，
            // 让剧情立刻按「未选择」（selectedIndex = -1）继续 —— 表现就是"还没选就跳过了"。
            NodeLog.Error(
                $"{GetType().Name}: 解析不到 OptionPanelController —— 选项不会显示，本次会按「未选择」" +
                "（selectedIndex = -1）继续。请检查「目标」端口接到的物体上是否挂了 OptionPanelController。");
            return;
        }

        List<string> texts = GetInputValue<List<string>>(nameof(options), options);
        List<Sprite> iconList = GetInputValue<List<Sprite>>(nameof(icons), icons);

        panel.SelectionMade += OnPanelSelectionMade;   // ★ 推模式：面板选中时同步回调

        panel.Show(texts, iconList);
        NodeLog.Info($"{GetType().Name}: 已显示 {panel.OptionCount} 个选项");
    }

    /// <summary>
    /// 面板选中时**同步**回调 —— 比轮询可靠：此刻面板必定还没被销毁，
    /// 所以即使面板勾了「选中后销毁自己」，索引也已经安全落袋。
    /// </summary>
    private void OnPanelSelectionMade(int index)
    {
        MarkResolved(index);
    }

    protected override bool CheckCondition()
    {
        CaptureIfReady();

        if (!waitForSelection) return true;      // 只显示，不在本节点等待
        return hasResolved || panel == null;     // 解析失败 fail-open，避免卡死剧情
    }

    /// <summary>
    /// 兜底：面板还活着时主动读一次选择结果。正常路径已由推模式的事件覆盖，
    /// 这里防的是"订阅之前就已经选好"之类的边界情况。
    /// </summary>
    private void CaptureIfReady()
    {
        if (hasResolved) return;
        if (panel == null || !panel.HasSelection) return;

        MarkResolved(panel.SelectedIndex);
    }

    /// <summary>
    /// 记录结果并**立刻退订**：结果已到手，后续无关的选择不该再覆盖它。
    /// 面板已销毁时 <c>panel != null</c> 为假会跳过退订 —— 无副作用（订阅随面板一起消失）。
    /// </summary>
    private void MarkResolved(int index)
    {
        resolvedIndex = index;
        hasResolved = true;

        if (panel != null) panel.SelectionMade -= OnPanelSelectionMade;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(options))
            return GetInputValue<List<string>>(nameof(options), options);

        if (port.fieldName == nameof(icons))
            return GetInputValue<List<Sprite>>(nameof(icons), icons);

        if (port.fieldName == nameof(selectionFinished))
        {
            CaptureIfReady();
            selectionFinished = hasResolved || panel == null;
            return selectionFinished;
        }

        if (port.fieldName == nameof(selectedIndex))
        {
            CaptureIfReady();
            selectedIndex = hasResolved ? resolvedIndex : (panel != null ? panel.SelectedIndex : -1);
            return selectedIndex;
        }

        return null;
    }

    /// <summary>
    /// 包一层基类流程，只为了**把"选项超时"变成显式警告**：
    /// "没选到任何项"是内容级决策，不该静默通过 —— 而 ConditionWaitNode 默认只打 Info（可能被日志级别过滤）。
    /// </summary>
    public override IEnumerator GetFlow()
    {
        yield return base.GetFlow();

        if (!waitForSelection || !TimedOut) yield break;

        NodePort branchPort = GetOutputPort(nameof(timeoutTo));
        bool hasBranch = branchPort != null && branchPort.IsConnected;

        NodeLog.Warning(
            $"{GetType().Name}: 选项等待超时（{timeout} 秒），**没有任何选择** → selectedIndex = -1，" +
            $"将沿 {(hasBranch ? "超时分支 timeoutTo" : "next（未接 timeoutTo）")} 继续。" +
            "若不想要超时：把 timeout 设为 0（不限时）；若要「超时走默认选项」：把 timeoutTo 接上并给它一条独立分支。");
    }
}
