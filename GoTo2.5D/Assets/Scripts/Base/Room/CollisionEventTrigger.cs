using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
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

    private bool IsValid(GameObject obj)
    {
        return string.IsNullOrEmpty(targetTag) || obj.CompareTag(targetTag);
    }
}