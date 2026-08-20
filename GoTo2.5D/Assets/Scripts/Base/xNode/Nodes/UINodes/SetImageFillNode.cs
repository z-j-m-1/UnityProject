using UnityEngine;
using UnityEngine.UI;
using XNode;

/// <summary>UI-设置图片填充（Image.fillAmount，0~1）</summary>
[CreateNodeMenu("UI/图片填充")]
public class SetImageFillNode : ComponentActionNode<Image>
{
    [Header("填充值（0~1）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public float fillAmount;

    protected override void Apply(Image image)
    {
        image.fillAmount = Mathf.Clamp01(GetInputValue<float>(nameof(fillAmount), fillAmount));
    }
}
