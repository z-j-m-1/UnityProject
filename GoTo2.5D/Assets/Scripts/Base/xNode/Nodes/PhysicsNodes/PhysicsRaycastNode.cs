using UnityEngine;
using XNode;

/// <summary>
/// 物理-射线检测（3D）：从 origin 沿 direction 发射射线，输出命中结果。
/// 同帧内多次读取共享同一次检测结果（帧缓存）。
/// </summary>
[CreateNodeMenu("物理/射线检测")]
[NodeTint("#44BBAA")]
public class PhysicsRaycastNode : DataNode
{
    [Header("射线起点")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 origin;

    [Header("方向（自动归一化）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 direction = Vector3.forward;

    [Header("最大距离")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float distance = 100f;

    [Header("层级")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public LayerMask layerMask = ~0;

    [Header("命中结果")]
    [Output] public bool isHit;
    [Output] public Vector3 hitPoint;
    [Output] public Vector3 hitNormal;
    [Output] public float hitDistance;
    [Output] public GameObject hitObject;

    private RaycastHit lastHit;
    private bool lastIsHit;
    private int lastFrame = -1;

    private void EnsureCast()
    {
        if (Time.frameCount == lastFrame) return;

        Vector3 o = GetInputValue<Vector3>(nameof(origin), origin);
        Vector3 d = GetInputValue<Vector3>(nameof(direction), direction);
        float dist = GetInputValue<float>(nameof(distance), distance);
        LayerMask mask = GetInputValue<LayerMask>(nameof(layerMask), layerMask);

        lastIsHit = Physics.Raycast(o, d, out lastHit, dist, mask);
        lastFrame = Time.frameCount;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(origin))
            return GetInputValue<Vector3>(nameof(origin), origin);
        if (port.fieldName == nameof(direction))
            return GetInputValue<Vector3>(nameof(direction), direction);
        if (port.fieldName == nameof(distance))
            return GetInputValue<float>(nameof(distance), distance);
        if (port.fieldName == nameof(layerMask))
            return GetInputValue<LayerMask>(nameof(layerMask), layerMask);

        EnsureCast();
        if (port.fieldName == nameof(isHit))
            return lastIsHit;
        if (port.fieldName == nameof(hitPoint))
            return lastHit.point;
        if (port.fieldName == nameof(hitNormal))
            return lastHit.normal;
        if (port.fieldName == nameof(hitDistance))
            return lastHit.distance;
        if (port.fieldName == nameof(hitObject))
            return lastIsHit && lastHit.collider != null ? lastHit.collider.gameObject : null;
        return null;
    }
}