using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 插值-移动到：目标 Transform 从当前位置在 duration 秒内插值到 targetPosition。
/// 继承 <see cref="TweenNodeBase"/> → 自带**曲线插值**（默认线性 = 与加曲线前一致）。
/// 结束精确归位到目标位置；曲线过冲时会先冲过再回来。
/// 目标物体由「目标物体」输入端口提供（接 取值/获取物体）。
/// </summary>
[CreateNodeMenu("插值/移动到")]
[NodeTint("#44AAFF")]
public class MoveToNode : TweenNodeBase
{
    [Header("目标位置")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 targetPosition;

    private Transform targetTransform;
    private Vector3 start;
    private Vector3 end;

    protected override bool ResolveTarget()
    {
        GameObject obj = ResolveTargetObject();
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把任一「取值/获取物体」节点接到目标端口）");
            return false;
        }

        targetTransform = obj.GetComponent<Transform>();
        if (targetTransform == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 没有 Transform");
            return false;
        }

        return true;
    }

    protected override bool CaptureStart()
    {
        start = targetTransform.position;
        end = GetInputValue<Vector3>(nameof(targetPosition), targetPosition);
        return true;
    }

    protected override void ApplyStep(float mix)
    {
        targetTransform.position = Mix(start, end, mix);
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(targetPosition))
            return GetInputValue<Vector3>(nameof(targetPosition), targetPosition);
        return base.GetValue(port);
    }
}
