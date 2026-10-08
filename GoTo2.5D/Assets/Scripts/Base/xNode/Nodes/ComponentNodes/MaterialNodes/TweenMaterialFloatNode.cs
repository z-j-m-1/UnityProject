using UnityEngine;
using XNode;

/// <summary>
/// 材质-浮点插值：把材质的某个浮点属性在 duration 秒内从起始值插值到目标值，
/// 走 <see cref="MaterialPropertyBlock"/>（不改材质资产、不实例化材质副本、每个物体独立）。
/// 继承 <see cref="TweenNodeBase"/> → 自带**曲线插值**（在节点面板上直接编辑曲线），
/// 适合溶解 / 描边 / 进度 / 泛光强度这类"慢慢变化"的参数。
///
/// 起始值：
///   · 默认读**当前有效值** —— 先看 PropertyBlock 上的覆盖（<c>HasFloat</c> 判定），没有才读材质资产的值；
///   · 勾选 <c>overrideStart</c> 则用 <c>startValue</c>（可直接填，也可接线）—— 串接多段插值时用得上。
///
/// ⚠️ 因为默认读"当前值"，**同一个节点连跑两次时，第二次的起始值就是第一次的结束值**：
///    目标值又没变 → from == to → 看起来"什么都没发生"。这是"从当前位置插到目标"的正常语义（不是 bug）。
///    要每次都从固定值重新播放：勾上 <c>overrideStart</c> 并填 <c>startValue</c>（例如 0）。
///    另外：即使 from == to，本节点**仍会照常走完 duration**，所以流程不会提前继续。
/// </summary>
[CreateNodeMenu("材质/浮点插值")]
[NodeTint("#000000")]
[NodeWidth(300)]
public class TweenMaterialFloatNode : TweenNodeBase
{
    [Header("属性名（如 _Dissolve / _OutlineWidth）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string propertyName;

    [Header("目标值")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float targetValue = 1f;

    [Header("起始值（勾选 = 用下面的值；不勾 = 读当前有效值）")]
    public bool overrideStart = false;

    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float startValue;

    private Renderer targetRenderer;
    private MaterialPropertyBlock block;
    private string prop;
    private float from;
    private float to;

    protected override bool ResolveTarget()
    {
        GameObject obj = ResolveTargetObject();
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把任一「取值/获取物体」节点接到目标端口）");
            return false;
        }

        targetRenderer = obj.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 上没有 Renderer");
            return false;
        }

        return true;
    }

    protected override bool CaptureStart()
    {
        prop = GetInputValue<string>(nameof(propertyName), propertyName);
        if (string.IsNullOrEmpty(prop))
        {
            NodeLog.Warning($"{GetType().Name}: 属性名为空");
            return false;
        }

        Material shared = targetRenderer.sharedMaterial;
        if (shared != null && !shared.HasProperty(prop))
        {
            NodeLog.Warning($"{GetType().Name}: 材质 '{shared.name}' 上没有属性 '{prop}'（检查拼写）");
            return false;
        }

        to = GetInputValue<float>(nameof(targetValue), targetValue);

        // 取回已有覆盖：既用来读起始值，也作为本节点写入的底本（避免整块替换掉别处写过的属性）
        if (block == null) block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);

        if (overrideStart)
        {
            from = GetInputValue<float>(nameof(startValue), startValue);
        }
        else if (block.HasFloat(prop))
        {
            from = block.GetFloat(prop);
        }
        else
        {
            from = shared != null ? shared.GetFloat(prop) : 0f;
        }

        if (Mathf.Approximately(from, to))
        {
            // 最常见的困惑：同一个节点连跑两次 —— 第二次的起始值就是第一次的结束值，
            // 目标值又没变 → 自然"看起来什么都没发生"。
            NodeLog.Warning(
                $"{GetType().Name}: 起始值与目标值相同（都是 {from}），本次插值不会产生任何可见变化。" +
                "常见原因：上一次插值已经把这个属性停在了目标值上。" +
                "若想每次都从固定值重新播放，请勾选 overrideStart 并把 startValue 填上（例如 0）。");
        }

        return true;
    }

    protected override void ApplyStep(float mix)
    {
        block.SetFloat(prop, Mix(from, to, mix));
        targetRenderer.SetPropertyBlock(block);
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(propertyName))
            return GetInputValue<string>(nameof(propertyName), propertyName);
        if (port.fieldName == nameof(targetValue))
            return GetInputValue<float>(nameof(targetValue), targetValue);
        if (port.fieldName == nameof(startValue))
            return GetInputValue<float>(nameof(startValue), startValue);
        return base.GetValue(port);
    }
}
