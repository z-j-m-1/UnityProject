using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>输入行为 → 三阶段事件映射条目（started/performed/canceled 独立事件槽）</summary>
[System.Serializable]
public class InputActionEventBinding
{
    [Tooltip("按键/组合键绑定（就地编辑；可配置 Press/Hold/MultiTap 等交互模式）")]
    public InputAction action;

    [Tooltip("交互开始（按下/开始满足交互条件时触发）")]
    public UnityEvent onStarted = new UnityEvent();

    [Tooltip("交互完成（默认=按下触发；Hold/MultiTap 等达到条件时触发）")]
    public UnityEvent onPerformed = new UnityEvent();

    [Tooltip("交互取消/结束（释放、中途放弃时触发）")]
    public UnityEvent onCanceled = new UnityEvent();

    [System.NonSerialized] internal bool isBound;
}

/// <summary>
/// 按键驱动 UI/事件 - 多绑定版：List 配置 N 个按键行为，每个行为三阶段事件槽。
/// 适用于键盘整排按键 / 多组合键 UI（面板按键、快捷栏等）。
/// 单按键场景请用 KeyInputEvent（更简洁，无需 List）。
/// </summary>
public class InputActionEventBinder : MonoBehaviour
{
    [Tooltip("按键行为列表（每个条目：InputAction + started/performed/canceled 事件槽）")]
    public List<InputActionEventBinding> bindings = new List<InputActionEventBinding>();

    void OnEnable()
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            Bind(bindings[i]);
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            Unbind(bindings[i]);
        }
    }

    private void Bind(InputActionEventBinding b)
    {
        if (b == null || b.action == null || b.isBound) return;

        b.action.started += OnStarted;
        b.action.performed += OnPerformed;
        b.action.canceled += OnCanceled;
        b.action.Enable();
        b.isBound = true;
    }

    private void Unbind(InputActionEventBinding b)
    {
        if (b == null || b.action == null || !b.isBound) return;

        b.action.started -= OnStarted;
        b.action.performed -= OnPerformed;
        b.action.canceled -= OnCanceled;
        b.action.Disable();
        b.isBound = false;
    }

    private void OnStarted(InputAction.CallbackContext ctx)
    {
        InputActionEventBinding b = FindBinding(ctx.action);
        if (b != null) b.onStarted?.Invoke();
    }

    private void OnPerformed(InputAction.CallbackContext ctx)
    {
        InputActionEventBinding b = FindBinding(ctx.action);
        if (b != null) b.onPerformed?.Invoke();
    }

    private void OnCanceled(InputAction.CallbackContext ctx)
    {
        InputActionEventBinding b = FindBinding(ctx.action);
        if (b != null) b.onCanceled?.Invoke();
    }

    /// <summary>按 action 查找条目（同一 action 被多条目引用时取第一个，应避免重复引用）</summary>
    private InputActionEventBinding FindBinding(InputAction action)
    {
        for (int i = 0; i < bindings.Count; i++)
        {
            if (bindings[i] != null && bindings[i].action == action)
            {
                return bindings[i];
            }
        }
        return null;
    }
}
