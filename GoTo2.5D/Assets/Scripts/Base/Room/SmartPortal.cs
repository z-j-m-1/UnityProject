using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class SmartPortal : MonoBehaviour
{
    [Header("目标传送门（二选一）")]
    [SerializeField] private SmartPortal targetPortal;          // 手动拖拽
    [SerializeField] private string targetPortalName;          // 或按名称自动查找
    
    [Header("出口位置（可选）")]
    [SerializeField] private Transform exitPoint;              // 不设置则使用目标传送门位置
    
    [Header("相机（可选）")]
    [SerializeField] private CinemachineVirtualCamera portalCamera;
    
    private void Start()
    {
        // 如果没手动设置目标，按名称查找
        if (targetPortal == null && !string.IsNullOrEmpty(targetPortalName))
        {
            GameObject found = GameObject.Find(targetPortalName);
            if (found != null) targetPortal = found.GetComponent<SmartPortal>();
        }
        
        // 如果没有出口点，使用传送门自身位置
        if (exitPoint == null) exitPoint = transform;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || targetPortal == null) return;
        
        // 传送
        Vector3 targetPos = targetPortal.exitPoint != null ? 
            targetPortal.exitPoint.position : targetPortal.transform.position;
        other.transform.position = targetPos;
        
        // 相机切换（如果有相机配置）
        if (portalCamera != null && targetPortal.portalCamera != null)
        {
            portalCamera.Priority = 10;
            targetPortal.portalCamera.Priority = 11;
        }
    }
}