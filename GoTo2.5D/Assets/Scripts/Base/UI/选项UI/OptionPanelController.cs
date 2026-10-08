using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 选项面板控制器 = 选项的**创建者** + 选择的**收集者**（合成一个脚本）。
///
/// 为什么合成一个：创建与收集**共享同一份状态**（选项列表、实例列表、索引映射）。
/// 拆成两个脚本就必须互相持有引用或互相查找，只有耦合成本、没有收益。
///
/// 图侧配合：<c>选项UI/显示选项</c> **一个节点搞定** ——
/// 它显示选项（<see cref="Show"/>）、在本节点内等待选择（轮询 <see cref="HasSelection"/>）、
/// 并把选择结果从输出端口（<see cref="SelectedIndex"/>）交给下游。
///
/// 三个刻意的设计：
///   · **默认复用而非销毁**：选项实例按需创建后只显隐，反复开关面板不会产生 Instantiate/Destroy 的 GC 峰值；
///     需要「一次性面板」时勾上 <c>destroySelfAfterSelection</c> —— 选中后销毁**本物体自己**
///     （多个面板物体各配不同选项排列时，用它区分"用完即弃"与"常驻复用"）；
///   · **不自动关闭**：Show 后不自动隐藏，选中后也不自动隐藏 —— 关闭时机交给节点图（便于先播"你选了 X"的反馈）；
///   · **Show 时确保可见**：panelRoot 没配就激活本物体，避免选项被创建在非激活父级下（看不见也点不到）。
/// </summary>
public class OptionPanelController : MonoBehaviour
{
    [Header("选项预设体（需挂 OptionUI）")]
    [SerializeField] private OptionUI optionPrefab;

    [Header("选项父节点（不填则用本物体）")]
    [SerializeField] private Transform optionParent;

    [Header("面板根（可选；填了就用它整块显隐）")]
    [SerializeField] private GameObject panelRoot;

    [Header("选中后销毁自己（本物体）—— 适合一次性面板；不勾选则保留面板待复用")]
    [SerializeField] private bool destroySelfAfterSelection = false;

    private readonly List<OptionUI> instances = new List<OptionUI>();

    private int selectedIndex = -1;
    private bool hasSelection;

    /// <summary>
    /// 选择完成时**同步**触发（在销毁自己之前），参数是选中索引。
    /// 节点靠它把索引"推"回来：若让节点轮询，面板可能已经被销毁，
    /// 再读 <c>SelectedIndex</c> 只会拿到 -1（Unity 假 null）—— 那正是"有时收到有时收不到"的根源。
    /// </summary>
    public event System.Action<int> SelectionMade;

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

        // ★ 确保可见（必须在创建实例之前）：panelRoot 没配时至少把本物体激活，
        //   否则选项实例会被创建在非激活的父级下 —— 玩家看不见、也点不到，只会一直等到超时。
        EnsureVisible();

        if (!parent.gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                $"{nameof(OptionPanelController)}: 选项父级 '{parent.name}' 在层级中处于非激活状态，" +
                "创建出来的选项既不可见也不可点击。", this);
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
    }

    /// <summary>
    /// 确保面板可见：配了 panelRoot 就用它；没配则至少激活本物体（选项实例的父级）。
    /// 不这样做的话，面板物体处于非激活时选项会被创建在非激活父级下 —— 看不见也点不到，
    /// 表现就是"选项没出来 / 点不动"，最后靠 timeout 才继续。
    /// </summary>
    private void EnsureVisible()
    {
        if (panelRoot != null)
        {
            if (!panelRoot.activeSelf) panelRoot.SetActive(true);
            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            Debug.LogWarning(
                $"{nameof(OptionPanelController)}: 面板物体原本是非激活的，Show 时已自动激活。" +
                "建议配置 panelRoot，或由对话流程显式开关面板。", this);
        }
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

        selectedIndex = option.Index;   // ★ 先记录选择，再考虑销毁 —— 销毁后仍要能读到 SelectedIndex
        hasSelection = true;

        for (int i = 0; i < instances.Count; i++)
        {
            if (instances[i] != null) instances[i].SetSelected(i == selectedIndex);
        }

        // ★ 先同步通知（节点在这里就把索引抓走），**再**销毁自己 —— 这两行的顺序不能反
        SelectionMade?.Invoke(selectedIndex);

        if (destroySelfAfterSelection) DestroySelf();
    }

    /// <summary>
    /// 销毁自己（本物体的 GameObject）。会先把自己创建的选项实例一并销毁 ——
    /// 因为 <c>optionParent</c> 不一定是自己的子物体，直接销毁自己可能留下孤儿选项。
    /// </summary>
    private void DestroySelf()
    {
        for (int i = 0; i < instances.Count; i++)
        {
            OptionUI option = instances[i];
            if (option == null) continue;

            option.Clicked -= OnOptionClicked;   // 先退订，避免销毁过程中再回调
            Destroy(option.gameObject);
        }

        instances.Clear();
        OptionCount = 0;

        Destroy(gameObject);
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
