using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;

/// <summary>
/// 通讯UI-获取文本节点（Text / TextMeshPro）
/// </summary>
[CreateNodeMenu("通讯UI/获取文本")]
public class ComUIGetTextNode : DataNode
{
    [Header("UI来源")]
    public UISource source = UISource.Self;

    [Header("UI类型")]
    public UIType uiType = UIType.TextMeshPro;

    [Header("UI对象名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string uiObjectName;

    [Header("剔除富文本")]
    public bool stripRichText = false;

    [Header("默认值")]
    public string defaultValue = "";

    [Output]
    public string outputValue;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(uiObjectName))
            return GetInputValue<string>(nameof(uiObjectName), uiObjectName);
        if (port.fieldName == nameof(outputValue))
        {
            string uiName = GetInputValue<string>(nameof(uiObjectName), uiObjectName);
            outputValue = defaultValue;

            if (string.IsNullOrEmpty(uiName))
            {
                if (Application.isPlaying)
                {
                    Debug.LogError($"{GetType().Name}: UI对象名称不能为空");
                }
                return outputValue;
            }

            string content = TryGetText(uiName);
            if (content != null)
            {
                outputValue = stripRichText ? RichTextUtility.Strip(content) : content;
            }
            else
            {
                if (Application.isPlaying)
                {
                    Debug.LogError($"{GetType().Name}: 未找到UI对象 '{uiName}'（{uiType}）");
                }
                // 编辑器模式下找不到属于正常（场景可能没有对应 UI / Self 源无附加物体），静默返回默认值
            }

            return outputValue;
        }
        return null;
    }

    /// <summary>返回 null 表示未找到或类型暂不支持</summary>
    private string TryGetText(string uiName)
    {
        BaseNodeGraph nodeGraph = graph as BaseNodeGraph;

        switch (uiType)
        {
            case UIType.Text:
                Text text = UIComponentResolver.Resolve<Text>(source, uiName, nodeGraph);
                return text != null ? text.text : null;

            case UIType.TextMeshPro:
                TMP_Text tmp = UIComponentResolver.Resolve<TMP_Text>(source, uiName, nodeGraph);
                return tmp != null ? tmp.text : null;

            case UIType.Image:
                Debug.LogWarning($"{GetType().Name}: Image 类型暂不支持文本获取");
                return null;

            default:
                return null;
        }
    }
}
