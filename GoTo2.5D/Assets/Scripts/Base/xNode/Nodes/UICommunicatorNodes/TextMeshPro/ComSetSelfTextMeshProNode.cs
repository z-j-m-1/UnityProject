using UnityEngine;
using XNode;
using TMPro;

/// <summary>
/// 通讯-设置自身TextMeshPro节点
/// </summary>
[CreateNodeMenu("通讯UI/设置自身TextMeshPro")]
public class ComSetSelfTextMeshProNode : ComSelfUISetNodeBase
{
    [Header("UI对象名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string uiObjectName;

    [Header("文本内容")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string textValue;

    public override void Execute()
    {
        if (graph is BaseNodeGraph nodeGraph && nodeGraph.attachedObject != null)
        {
            string graphName = nodeGraph.name;
            string uiName = GetInputValue<string>("uiObjectName", uiObjectName);
            string content = GetInputValue<string>("textValue", textValue);

            if (string.IsNullOrEmpty(uiName))
            {
                Debug.LogError("ComSetSelfTextMeshProNode: UI对象名称不能为空");
                base.Execute();
                return;
            }

            // UICommunicator负责查找和缓存UI对象
            UICommunicator.Instance.GetOrCreateCache(graphName, uiName, nodeGraph.attachedObject);

            // 触发事件设置
            ComSetSelfTextMeshProEvent.Trigger(evt =>
            {
                evt.graphName = graphName;
                evt.uiObjectName = uiName;
                evt.textValue = content;
            });

            Debug.Log($"ComSetSelfTextMeshProNode: 设置自身TextMeshPro '{uiName}' = '{content}'");
        }
        else
        {
            Debug.LogError("ComSetSelfTextMeshProNode: 节点图未附加到任何GameObject");
        }
        base.Execute();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(uiObjectName))
            return GetInputValue<string>("uiObjectName", uiObjectName);
        if (port.fieldName == nameof(textValue))
            return GetInputValue<string>("textValue", textValue);
        return null;
    }
}