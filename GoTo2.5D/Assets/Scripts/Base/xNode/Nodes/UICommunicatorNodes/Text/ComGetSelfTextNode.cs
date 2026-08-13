using UnityEngine;
using XNode;
using UnityEngine.UI;

/// <summary>
/// 通讯-获取自身Text节点
/// </summary>
[CreateNodeMenu("通讯UI/获取自身Text")]
public class ComGetSelfTextNode : ComSelfUIGetNodeBase
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
                    Debug.LogError("ComGetSelfTextNode: UI对象名称不能为空");
                    return defaultValue;
                }

                // UICommunicator负责查找和缓存UI对象
                UICommunicator.Instance.GetOrCreateCache(graphName, uiName, nodeGraph.attachedObject);

                // 触发事件获取
                ComGetSelfTextEvent.Trigger(evt =>
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