using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 选项面板控制器 = 选项的**创建者** + 选择的**收集者**（合成一个脚本）。
///
/// 为什么合成一个：创建与收集**共享同一份状态**（选项列表、实例列表、索引映射）。
/// 拆成两个脚本就必须互相持有引用或互相查找，只有耦合成本、没有收益。
///
/// 图侧配合（3 个节点）：
///   <c>选项UI/显示选项</c>     → <see cref="Show"/>
///   <c>选项UI/等待选择</c>     → 轮询 <see cref="HasSelection"/>（继承 ConditionWaitNode，白拿 timeout + 超时分支）
///   <c>选项UI/获取选中索引</c> → 读 <see cref="SelectedIndex"/>
///
/// 两个刻意的设计：
///   · **复用而非销毁**：选项实例按需创建后只显隐，反复开关面板不会产生 Instantiate/Destroy 的 GC 峰值；
///   · **不自动关闭**：Show 后不自动隐藏，选中后也不自动隐藏 —— 关闭时机交给节点图（便于先播"你选了 X"的反馈）。
/// </summary>
public class OptionPanelController : MonoBehaviour
{
    [Header("选项预设体（需挂 OptionUI）")]
    [SerializeField] private OptionUI optionPrefab;

    [Header("选项父节点（不填则用本物体）")]
    [SerializeField] private Transform optionParent;

    [Header("面板根（可选；填了就用它整块显隐）")]
    [SerializeField] private GameObject panelRoot;

    private readonly List<OptionUI> instances = new List<OptionUI>();

    private int selectedIndex = -1;
    private bool hasSelection;

    /// <summary>选择是否已完成（= 「等待选择」节点的条件）</summary>
    public bool HasSelection => hasSelection;

    /// <summary>选中的索引；-1 = 尚未选择</summary>
    public int SelectedIndex => selectedIndex;

    /// <summary>当前展示的选项数量</summary>
    public int OptionCount { get; private set; }

    /// <summary>
    /// 显示一组选项（文本；图标可选，缺省或数量不足的项不显示图标）。
    /// 重复调用会复用已有实例；**每次都会重置选择状态**。
    /// </summary>
    public void Show(IList<string> options, IList<Sprite> icons = null)
    {
        Transform parent = optionParent != null ? optionParent : transform;

        if (optionPrefab == null)
        {
            Debug.LogError($"{nameof(OptionPanelController)}: 未配置选项预设体，无法显示选项", this);
            OptionCount = 0;
            hasSelection = false;
            selectedIndex = -1;
            return;
        }

        // ★ 每次 Show 都必须重置选择状态，否则上一轮的选择会让「等待选择」立刻通过
        hasSelection = false;
        selectedIndex = -1;

        int count = options != null ? options.Count : 0;
        if (count == 0)
        {
            Debug.LogWarning(
                $"{nameof(OptionPanelController)}: 选项列表为空 —— 「选项UI/等待选择」将永远等不到" +
                "（除非该节点配了 timeout + 超时分支）", this);
        }

        EnsureInstanceCount(count, parent);

        for (int i = 0; i < instances.Count; i++)
        {
            OptionUI option = instances[i];
            bool used = i < count;

            if (option.gameObject.activeSelf != used) option.gameObject.SetActive(used);
            if (!used) continue;

            Sprite icon = icons != null && i < icons.Count ? icons[i] : null;
            option.Setup(i, options[i], icon);
            option.SetSelected(false);
        }

        OptionCount = count;
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    /// <summary>隐藏面板（只显隐，不销毁实例）</summary>
    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
            return;
        }

        for (int i = 0; i < instances.Count; i++)
        {
            if (instances[i] != null) instances[i].gameObject.SetActive(false);
        }
    }

    /// <summary>手动清空选择状态（Show 会自动清；需要中途重开一轮时用）</summary>
    public void ClearSelection()
    {
        hasSelection = false;
        selectedIndex = -1;

        for (int i = 0; i < instances.Count; i++)
        {
            if (instances[i] != null) instances[i].SetSelected(false);
        }
    }

    /// <summary>按需补足实例（只增不减，多余的隐藏起来备用）</summary>
    private void EnsureInstanceCount(int count, Transform parent)
    {
        while (instances.Count < count)
        {
            OptionUI option = Instantiate(optionPrefab, parent);
            option.name = $"{optionPrefab.name}_{instances.Count}";
            option.Clicked += OnOptionClicked;   // 只订阅自己的事件，设计者挂的 onClick 不受影响
            instances.Add(option);
        }
    }

    private void OnOptionClicked(OptionUI option)
    {
        if (hasSelection) return;   // 已选完 → 忽略后续点击（防连点）

        selectedIndex = option.Index;
        hasSelection = true;

        for (int i = 0; i < instances.Count; i++)
        {
            if (instances[i] != null) instances[i].SetSelected(i == selectedIndex);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < instances.Count; i++)
        {
            if (instances[i] != null) instances[i].Clicked -= OnOptionClicked;
        }
        instances.Clear();
    }
}
