#if UNITY_EDITOR
using UnityEditor;
using XNode;
using XNodeEditor;

/// <summary>
/// 所有 BaseNode 的默认节点体编辑器：
/// 复刻 xNode 默认 OnBodyGUI + 运行时高亮亮边。
/// 覆盖没有专属编辑器的节点（分支/逻辑/数学/字符串/取值/变换/UI 等）。
/// </summary>
[CustomNodeEditor(typeof(BaseNode))]
public class BaseNodeBodyEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        BaseNode baseNode = target as BaseNode;
        bool highlighted = NodeRunHighlight.BeginIfActive(baseNode);

        serializedObject.Update();
        string[] excludes = { "m_Script", "graph", "position", "ports" };
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (System.Array.IndexOf(excludes, iterator.name) >= 0) continue;
            NodeEditorGUILayout.PropertyField(iterator, true);
        }
        foreach (XNode.NodePort dynamicPort in target.DynamicPorts)
        {
            if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
            NodeEditorGUILayout.PortField(dynamicPort);
        }
        serializedObject.ApplyModifiedProperties();

        if (highlighted)
        {
            NodeRunHighlight.EndHighlight();
        }
    }
}
#endif
