using UnityEngine;
using XNode;

/// <summary>
/// 物理-射线检测（2D）：从 origin 沿 direction 发射 2D 射线，输出命中结果。
/// 同帧内多次读取共享同一次检测结果（帧缓存）。
/// </summary>
[CreateNodeMenu("物理/射线检测(2D)")]
[NodeTint("#44BBAA")]
public class PhysicsRaycast2DNode : DataNode
{
    [Header("射线起点")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector2 origin;

    [Header("方向（自动归一化）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector2 direction = Vector2.right;

    [Header("最大距离")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float distance = 100f;

    [Header("层级")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public LayerMask layerMask = ~0;

    [Header("命中结果")]
    [Output] public bool isHit;
    [Output] public Vector2 hitPoint;
    [Output] public Vector2 hitNormal;
    [Output] public float hitDistance;
    [Output] public GameObject hitObject;

    private RaycastHit2D lastHit;
    private bool lastIsHit;
    private int lastFrame = -1;

    private void EnsureCast()
    {
        if (Time.frameCount == lastFrame) return;

        Vector2 o = GetInputValue<Vector2>(nameof(origin), origin);
        Vector2 d = GetInputValue<Vector2>(nameof(direction), direction);
        float dist = GetInputValue<float>(nameof(distance), distance);
        LayerMask mask = GetInputValue<LayerMask>(nameof(layerMask), layerMask);

        lastHit = Physics2D.Raycast(o, d, dist, mask);
        lastIsHit = lastHit.collider != null;
        lastFrame = Time.frameCount;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(origin))
            return GetInputValue<Vector2>(nameof(origin), origin);
        if (port.fieldName == nameof(direction))
            return GetInputValue<Vector2>(nameof(direction), direction);
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