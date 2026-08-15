#if UNITY_EDITOR
using UnityEditor;
using XNode;
using XNodeEditor;

/// <summary>
/// Get/Set 变量节点 + 入口节点的节点体编辑器：
/// 在节点图视图中隐藏 variableGuid / guid（避免误触），检查器里仍正常显示
/// </summary>
public abstract class VariableNodeBodyEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        serializedObject.Update();
        string[] excludes = { "m_Script", "graph", "position", "ports", "variableGuid", "guid" };
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
    }
}

/// <summary>获取变量节点：节点体隐藏 variableGuid</summary>
[CustomNodeEditor(typeof(GetVariableNodeBase))]
public class GetVariableNodeBodyEditor : VariableNodeBodyEditor
{
}

/// <summary>设置变量节点：节点体隐藏 variableGuid</summary>
[CustomNodeEditor(typeof(SetVariableNodeBase))]
public class SetVariableNodeBodyEditor : VariableNodeBodyEditor
{
}

/// <summary>入口节点：节点体隐藏 guid</summary>
[CustomNodeEditor(typeof(EntryNode))]
public class EntryNodeBodyEditor : VariableNodeBodyEditor
{
}
#endif
