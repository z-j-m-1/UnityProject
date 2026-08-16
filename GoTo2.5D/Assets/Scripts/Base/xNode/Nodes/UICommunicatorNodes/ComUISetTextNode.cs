using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XNode;

/// <summary>
/// 通讯UI-设置文本节点（Text / TextMeshPro）
/// </summary>
[CreateNodeMenu("通讯UI/设置文本")]
public class ComUISetTextNode : FlowNode
{
    [Header("UI来源")]
    public UISource source = UISource.Self;

    [Header("UI类型")]
    public UIType uiType = UIType.TextMeshPro;

    [Header("UI对象名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string uiObjectName;

    [Header("文本内容")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string textValue;

    public override void Execute()
    {
        string uiName = GetInputValue<string>(nameof(uiObjectName), uiObjectName);
        string content = GetInputValue<string>(nameof(textValue), textValue);

        if (string.IsNullOrEmpty(uiName))
        {
            Debug.LogError($"{GetType().Name}: UI对象名称不能为空");
            return;
        }

        BaseNodeGraph nodeGraph = graph as BaseNodeGraph;

        switch (uiType)
        {
            case UIType.Text:
                Text text = UIComponentResolver.Resolve<Text>(source, uiName, nodeGraph);
                if (text != null)
                {
                    text.text = content;
                    NodeLog.Info($"{GetType().Name}: 设置文本 '{uiName}' = '{content}'");
                }
                else
                {
                    Debug.LogError($"{GetType().Name}: 未找到UI对象 '{uiName}'（Text）");
                }
                break;

            case UIType.TextMeshPro:
                TMP_Text tmp = UIComponentResolver.Resolve<TMP_Text>(source, uiName, nodeGraph);
                if (tmp != null)
                {
                    tmp.text = content;
                    NodeLog.Info($"{GetType().Name}: 设置文本 '{uiName}' = '{content}'");
                }
                else
                {
                    Debug.LogError($"{GetType().Name}: 未找到UI对象 '{uiName}'（TextMeshPro）");
                }
                break;

            case UIType.Image:
                Debug.LogWarning($"{GetType().Name}: Image 类型暂不支持文本设置");
                break;
        }

        // 执行下一个节点
        base.Execute();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(uiObjectName))
            return GetInputValue<string>(nameof(uiObjectName), uiObjectName);
        if (port.fieldName == nameof(textValue))
            return GetInputValue<string>(nameof(textValue), textValue);
        return null;
    }
}
