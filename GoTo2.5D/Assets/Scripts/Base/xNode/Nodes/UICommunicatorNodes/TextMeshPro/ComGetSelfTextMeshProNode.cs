using UnityEngine;
using XNode;
using TMPro;

/// <summary>
/// 通讯-获取自身TextMeshPro节点
/// </summary>
[CreateNodeMenu("通讯UI/获取自身TextMeshPro")]
public class ComGetSelfTextMeshProNode : ComSelfUIGetNodeBase
{
    [Header("UI对象名称")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public string uiObjectName;

    [Output]
    public string outputValue;

    [Header("默认值")]
    public string defaultValue = "";

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(uiObjectName))
            return GetInputValue<string>("uiObjectName", uiObjectName);

        if (port.fieldName == nameof(outputValue))
        {
            if (graph is BaseNodeGraph nodeGraph && nodeGraph.attachedObject != null)
            {
                string graphName = nodeGraph.name;
                string uiName = GetInputValue<string>("uiObjectName", uiObjectName);

                if (string.IsNullOrEmpty(uiName))
                {
                    Debug.LogError("ComGetSelfTextMeshProNode: UI对象名称不能为空");
                    return defaultValue;
                }

                // UICommunicator负责查找和缓存UI对象
                UICommunicator.Instance.GetOrCreateCache(graphName, uiName, nodeGraph.attachedObject);

                // 触发事件获取
                ComGetSelfTextMeshProEvent.Trigger(evt =>
                {
                    evt.graphName = graphName;
                    evt.uiObjectName = uiName;
                    evt.defaultValue = defaultValue;
                    evt.callback = (value) =>
                    {
                        outputValue = value;
                    };
                });
                return outputValue;
            }
            return defaultValue;
        }
        return null;
    }
}