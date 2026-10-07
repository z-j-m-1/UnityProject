using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 选项 UI（**单个选项的视图**，挂在选项预设体上）：与对话框脚本类似，
/// 持有边框 / 背景 / 文本（+ 图标）引用。
///
/// **继承 <see cref="Button"/>**（而不是裸实现 IPointerClickHandler）：
///   · Button 本身就实现了 IPointerClickHandler → 点击事件白拿；
///   · 同时白拿 悬停/按下的视觉状态（Color Tint / Sprite Swap）—— 正是选项高亮需要的；
///   · 还有 interactable（禁用某一项）。
///   需要长按/悬停等额外指针事件时，照 <c>LongPressButton</c> 的写法加接口即可。
///
/// 职责边界：本脚本只表示"**我是不是被选中的那一个**"（用于视觉高亮），
/// **不持有"选择完毕"标志** —— 那属于收集者 <c>OptionPanelController</c>。
/// </summary>
[AddComponentMenu("UI/Option UI", 32)]
public class OptionUI : Button
{
    [Header("UI 组件引用")]
    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text optionText;
    [SerializeField] private Image iconImage;

    [Header("选中标记（可选；也可以直接用 Button 的 Color Tint）")]
    [SerializeField] private GameObject selectedMark;

    /// <summary>本选项的索引（由收集者 Setup 时写入）</summary>
    public int Index { get; private set; }

    /// <summary>当前是否是"被选中"的那一个</summary>
    public bool IsSelected { get; private set; }

    /// <summary>当前显示的文本</summary>
    public string OptionText => optionText != null ? optionText.text : string.Empty;

    /// <summary>点击回调（由收集者订阅；**不动设计者在 prefab 上挂的 onClick**，点击音效等照常生效）</summary>
    public event System.Action<OptionUI> Clicked;

    /// <summary>由收集者设置内容；icon 传 null 会隐藏图标</summary>
    public void Setup(int index, string text, Sprite icon = null)
    {
        Index = index;

        if (optionText != null) optionText.text = text;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
    }

    /// <summary>设置选中态（视觉高亮）</summary>
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (selectedMark != null) selectedMark.SetActive(selected);
    }

    /// <summary>
    /// 先走基类（触发 onClick：保留设计者挂的点击音效等），再抛自己的 Clicked 事件。
    /// 与 LongPressButton 覆写 OnPointerClick 的写法一致。
    /// </summary>
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (IsActive() && IsInteractable())
        {
            Clicked?.Invoke(this);
        }
    }
}
