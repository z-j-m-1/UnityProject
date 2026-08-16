using UnityEngine;
using XNode;

/// <summary>
/// 物理-球形检测（3D）：以 center 为中心 radius 半径做 OverlapSphere，输出命中数量与按索引取命中物体/位置。
/// 同帧内多次读取共享同一次检测结果（帧缓存）。
/// </summary>
[CreateNodeMenu("物理/球形检测")]
[NodeTint("#44BBAA")]
public class PhysicsOverlapSphereNode : DataNode
{
    [Header("球心")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 center;

    [Header("半径")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float radius = 0.5f;

    [Header("层级")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public LayerMask layerMask = ~0;

    [Header("命中索引（取第几个命中物体）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public int index;

    [Header("命中结果")]
    [Output] public int hitCount;
    [Output] public GameObject hitObject;
    [Output] public Vector3 hitPoint;

    private Collider[] hits;
    private int lastCount;
    private int lastFrame = -1;

    private void EnsureCast()
    {
        if (Time.frameCount == lastFrame) return;

        Vector3 c = GetInputValue<Vector3>(nameof(center), center);
        float r = GetInputValue<float>(nameof(radius), radius);
        LayerMask mask = GetInputValue<LayerMask>(nameof(layerMask), layerMask);

        hits = Physics.OverlapSphere(c, r, mask);
        lastCount = hits != null ? hits.Length : 0;
        lastFrame = Time.frameCount;
    }

    private Collider GetHitAt(int idx)
    {
        if (hits == null || idx < 0 || idx >= lastCount)
        {
            NodeLog.Warning($"{GetType().Name}: 索引 {idx} 越界（命中 {lastCount} 个），返回空");
            return null;
        }
        return hits[idx];
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(center))
            return GetInputValue<Vector3>(nameof(center), center);
        if (port.fieldName == nameof(radius))
            return GetInputValue<float>(nameof(radius), radius);
        if (port.fieldName == nameof(layerMask))
            return GetInputValue<LayerMask>(nameof(layerMask), layerMask);
        if (port.fieldName == nameof(index))
            return GetInputValue<int>(nameof(index), index);

        EnsureCast();
        if (port.fieldName == nameof(hitCount))
            return lastCount;
        if (port.fieldName == nameof(hitObject))
        {
            Collider c = GetHitAt(GetInputValue<int>(nameof(index), index));
            return c != null ? c.gameObject : null;
        }
        if (port.fieldName == nameof(hitPoint))
        {
            Collider c = GetHitAt(GetInputValue<int>(nameof(index), index));
            return c != null ? c.transform.position : Vector3.zero;
        }
        return null;
    }
}