using UnityEngine;
using UnityEngine.UI;
using XNode;

/// <summary>UI-读取图片填充值（Image.fillAmount，0~1）</summary>
[CreateNodeMenu("UI/图片填充值")]
public class GetImageFillNode : ComponentActionNodeBase
{
    [Output]
    public float fillAmount;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName != nameof(fillAmount))
            return null;

        GameObject obj = ResolveTargetObject();
        if (obj == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把「取值/获取物体(自身|名称|引用)」接到目标端口）");
            return 0f;
        }

        Image image = obj.GetComponent<Image>();
        if (image == null)
        {
            NodeLog.Warning($"{GetType().Name}: 目标 '{obj.name}' 上没有 Image 组件");
            return 0f;
        }

        return image.fillAmount;
    }
}
