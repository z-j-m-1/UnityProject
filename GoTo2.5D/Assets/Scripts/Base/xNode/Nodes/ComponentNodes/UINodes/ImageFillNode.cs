using UnityEngine;
using UnityEngine.UI;
using XNode;

/// <summary>图片填充操作</summary>
public enum ImageFillMode
{
    /// <summary>设置填充值（Image.fillAmount，自动 Clamp 到 0~1）</summary>
    Set,
    /// <summary>读取当前填充值（走输出端口 currentFill）</summary>
    Get
}

/// <summary>
/// UI-图片填充：**一个节点覆盖设置与读取**，模式枚举切换
/// （原「UI/图片填充」SetImageFillNode 与「UI/图片填充值」GetImageFillNode 已合并删除）。
///
/// 读取模式也走流程节点（与原 GetImageFillNode 一致）：本节点是 FlowNode，
/// 「当前填充值」是从它自身的输出端口读的。
/// 目标物体由「目标物体」输入端口提供（接 取值/获取物体）。
/// </summary>
[CreateNodeMenu("UI/图片填充")]
public class ImageFillNode : ComponentActionNodeBase
{
    [Header("操作")]
    public ImageFillMode mode = ImageFillMode.Set;

    [Header("填充值（0~1，设置模式使用）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float fillAmount;

    [Header("当前填充值（读取模式使用）")]
    [Output]
    public float currentFill;

    public override void Execute()
    {
        if (mode != ImageFillMode.Set) return;

        Image image = ResolveImage();
        if (image == null) return;

        image.fillAmount = Mathf.Clamp01(GetInputValue<float>(nameof(fillAmount), fillAmount));
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(fillAmount))
            return GetInputValue<float>(nameof(fillAmount), fillAmount);

        if (port.fieldName == nameof(currentFill))
        {
            Image image = ResolveImage();
            currentFill = image != null ? image.fillAmount : 0f;
            return currentFill;
        }

        return null;
    }

    private Image ResolveImage()
    {
        GameObject obj = ResolveTargetObject();
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把任一「取值/获取物体」节点接到目标端口）");
            return null;
        }

        Image image = obj.GetComponent<Image>();
        if (image == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 上没有 Image 组件");
        }
        return image;
    }
}
