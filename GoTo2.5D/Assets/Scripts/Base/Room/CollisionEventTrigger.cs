using UnityEngine;
using UnityEngine.Events;

public class CollisionEventTrigger : MonoBehaviour
{
    [Header("触发时机")]
    [SerializeField] private bool triggerOnEnter = true;
    [SerializeField] private bool triggerOnStay = false;
    [SerializeField] private bool triggerOnExit = false;

    [Header("标签过滤（留空则不过滤）")]
    [SerializeField] private string targetTag = "Player";

    [Header("事件")]
    public UnityEvent onEnter;
    public UnityEvent onStay;
    public UnityEvent onExit;

    // ==================== 3D 碰撞 ====================
    private void OnCollisionEnter(Collision other)
    {
        if (triggerOnEnter && IsValid(other.gameObject)) onEnter?.Invoke();
    }

    private void OnCollisionStay(Collision other)
    {
        if (triggerOnStay && IsValid(other.gameObject)) onStay?.Invoke();
    }

    private void OnCollisionExit(Collision other)
    {
        if (triggerOnExit && IsValid(other.gameObject)) onExit?.Invoke();
    }

    // ==================== 3D 触发 ====================
    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnEnter && IsValid(other.gameObject)) onEnter?.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        if (triggerOnStay && IsValid(other.gameObject)) onStay?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (triggerOnExit && IsValid(other.gameObject)) onExit?.Invoke();
    }

    // ==================== 2D 碰撞 ====================
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (triggerOnEnter && IsValid(other.gameObject)) onEnter?.Invoke();
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (triggerOnStay && IsValid(other.gameObject)) onStay?.Invoke();
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (triggerOnExit && IsValid(other.gameObject)) onExit?.Invoke();
    }

    // ==================== 2D 触发 ====================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnEnter && IsValid(other.gameObject)) onEnter?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (triggerOnStay && IsValid(other.gameObject)) onStay?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (triggerOnExit && IsValid(other.gameObject)) onExit?.Invoke();
    }

    // ==================== 通用 ====================
    private bool IsValid(GameObject obj)
    {
        return string.IsNullOrEmpty(targetTag) || obj.CompareTag(targetTag);
    }
}