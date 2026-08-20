using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// 按键驱动 UI/事件 - 单绑定版：一个 InputAction + 三阶段事件槽（started/performed/canceled）。
/// 适用于单个按键/组合键触发单个 UI 反应（高亮/音效/进图执行）。
/// 多按键场景请用 InputActionEventBinder（List 版）。
/// action 可就地配置交互模式（Press/Hold/Tap/MultiTap 等），判定由 InputSystem 完成。
/// </summary>
public class KeyInputEvent : MonoBehaviour
{
    [Tooltip("按键/组合键绑定（就地编辑；可配置 Press/Hold/MultiTap 等交互模式）")]
    public InputAction action;

    [Tooltip("交互开始（按下/开始满足交互条件时触发）")]
    public UnityEvent onStarted = new UnityEvent();

    [Tooltip("交互完成（默认=按下触发；Hold/MultiTap 等达到条件时触发）")]
    public UnityEvent onPerformed = new UnityEvent();

    [Tooltip("交互取消/结束（释放、中途放弃时触发）")]
    public UnityEvent onCanceled = new UnityEvent();

    private bool isBound;

    void OnEnable()
    {
        Bind();
    }

    void OnDisable()
    {
        Unbind();
    }

    private void Bind()
    {
        if (action == null || isBound) return;

        action.started += OnStarted;
        action.performed += OnPerformed;
        action.canceled += OnCanceled;
        action.Enable();
        isBound = true;
    }

    private void Unbind()
    {
        if (action == null || !isBound) return;

        action.started -= OnStarted;
        action.performed -= OnPerformed;
        action.canceled -= OnCanceled;
        action.Disable();
        isBound = false;
    }

    private void OnStarted(InputAction.CallbackContext ctx) => onStarted?.Invoke();

    private void OnPerformed(InputAction.CallbackContext ctx) => onPerformed?.Invoke();

    private void OnCanceled(InputAction.CallbackContext ctx) => onCanceled?.Invoke();
}
