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
