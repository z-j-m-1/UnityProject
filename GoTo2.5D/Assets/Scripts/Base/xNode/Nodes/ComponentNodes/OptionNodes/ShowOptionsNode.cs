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

    /// <summary>等待开始前：解析面板并显示选项（ConditionWaitNode.Execute 会调这里）</summary>
    protected override void OnWaitStart()
    {
        panel = NodeTargetResolver.Resolve<OptionPanelController>(this);
        if (panel == null) return;

        List<string> texts = GetInputValue<List<string>>(nameof(options), options);
        List<Sprite> iconList = GetInputValue<List<Sprite>>(nameof(icons), icons);

        panel.Show(texts, iconList);
        NodeLog.Info($"{GetType().Name}: 已显示 {panel.OptionCount} 个选项");
    }

    protected override bool CheckCondition()
    {
        if (!waitForSelection) return true;            // 只显示，不在本节点等待
        return panel == null || panel.HasSelection;    // 解析失败 fail-open，避免卡死剧情
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(options))
            return GetInputValue<List<string>>(nameof(options), options);

        if (port.fieldName == nameof(icons))
            return GetInputValue<List<Sprite>>(nameof(icons), icons);

        if (port.fieldName == nameof(selectionFinished))
        {
            selectionFinished = panel == null || panel.HasSelection;
            return selectionFinished;
        }

        if (port.fieldName == nameof(selectedIndex))
        {
            selectedIndex = panel != null ? panel.SelectedIndex : -1;
            return selectedIndex;
        }

        return null;
    }
}
