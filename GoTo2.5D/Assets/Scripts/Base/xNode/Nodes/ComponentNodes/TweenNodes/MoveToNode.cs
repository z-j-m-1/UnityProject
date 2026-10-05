using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 插值-移动到：目标 Transform 从当前位置在 duration 秒内插值到 targetPosition（GetFlow 逐帧，结束精确归位）。
/// 目标解析复用 ComponentActionNodeBase（GameObject 输入端口 &gt; Attached/ByName/Direct）。
/// </summary>
[CreateNodeMenu("插值/移动到")]
[NodeTint("#44AAFF")]
public class MoveToNode : ComponentActionNodeBase
{
    [Header("目标位置")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 targetPosition;

    [Header("持续时间（秒）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float duration = 1f;

    private Transform targetTransform;

    public override void Execute()
    {
        GameObject obj = ResolveTargetObject();
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未解析到目标物体（{target}）");
            return;
        }
        targetTransform = obj.GetComponent<Transform>();
        if (targetTransform == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 没有 Transform");
        }
    }

    public override IEnumerator GetFlow()
    {
        if (targetTransform == null) yield break;

        Vector3 start = targetTransform.position;
        Vector3 end = GetInputValue<Vector3>(nameof(targetPosition), targetPosition);
        float dur = Mathf.Max(0f, GetInputValue<float>(nameof(duration), duration));

        float t0 = Time.time;
        while (Time.time - t0 < dur)
        {
            float k = dur > 0f ? Mathf.Clamp01((Time.time - t0) / dur) : 1f;
            targetTransform.position = Vector3.Lerp(start, end, k);
            yield return null;
        }
        targetTransform.position = end;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(targetPosition))
            return GetInputValue<Vector3>(nameof(targetPosition), targetPosition);
        if (port.fieldName == nameof(duration))
            return GetInputValue<float>(nameof(duration), duration);
        return null;
    }
}