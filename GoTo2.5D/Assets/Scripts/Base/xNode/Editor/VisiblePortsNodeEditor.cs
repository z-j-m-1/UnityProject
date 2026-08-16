#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

/// <summary>
/// 节点体编辑器基类：与 xNode 默认 OnBodyGUI 相同，额外补画 [NonSerialized] 字段的端口
/// （Unity 序列化迭代器不含非序列化字段，默认编辑器会漏掉 GameObject 等非序列化端口）。
/// 子类可覆写 OnDrawProperty / OnBodyFooter 定制。
/// </summary>
public abstract class VisiblePortsNodeEditor : NodeEditor
{
    private readonly HashSet<string> renderedProps = new HashSet<string>();

    public override void OnBodyGUI()
    {
        serializedObject.Update();
        string[] excludes = { "m_Script", "graph", "position", "ports" };
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        renderedProps.Clear();
        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (System.Array.IndexOf(excludes, iterator.name) >= 0) continue;
            renderedProps.Add(iterator.name);
            if (OnDrawProperty(iterator)) continue;
            NodeEditorGUILayout.PropertyField(iterator, true);
        }

        foreach (NodePort dynamicPort in target.DynamicPorts)
        {
            if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
            NodeEditorGUILayout.PortField(dynamicPort);
        }

        // 补画非序列化字段的端口（默认编辑器漏掉：GameObject 端口等）
        foreach (FieldInfo field in target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (!field.IsDefined(typeof(XNode.Node.InputAttribute), false) && !field.IsDefined(typeof(XNode.Node.OutputAttribute), false)) continue;
            if (renderedProps.Contains(field.Name)) continue;
            NodePort port = target.GetPort(field.Name);
            if (port != null && !port.IsDynamic)
            {
                NodeEditorGUILayout.PortField(port);
            }
        }

        serializedObject.ApplyModifiedProperties();
        OnBodyFooter();
    }

    /// <summary>序列化属性绘制前回调；返回 true 表示已自行绘制（跳过默认）</summary>
    protected virtual bool OnDrawProperty(SerializedProperty property) => false;

    /// <summary>节点体末尾（运行时信息等）</summary>
    protected virtual void OnBodyFooter() { }
}

[CustomNodeEditor(typeof(ComponentActionNodeBase))]
public class ComponentActionNodeBodyEditor : VisiblePortsNodeEditor { }

[CustomNodeEditor(typeof(GetGameObjectNode))]
public class GetGameObjectNodeBodyEditor : VisiblePortsNodeEditor { }

[CustomNodeEditor(typeof(SpawnObjectNode))]
public class SpawnObjectNodeBodyEditor : VisiblePortsNodeEditor { }

[CustomNodeEditor(typeof(DestroyObjectNode))]
public class DestroyObjectNodeBodyEditor : VisiblePortsNodeEditor { }

[CustomNodeEditor(typeof(PhysicsRaycastNode))]
public class PhysicsRaycastNodeBodyEditor : VisiblePortsNodeEditor { }

[CustomNodeEditor(typeof(PhysicsRaycast2DNode))]
public class PhysicsRaycast2DNodeBodyEditor : VisiblePortsNodeEditor { }

[CustomNodeEditor(typeof(PhysicsOverlapSphereNode))]
public class PhysicsOverlapSphereNodeBodyEditor : VisiblePortsNodeEditor { }

[CustomNodeEditor(typeof(PhysicsOverlapCircleNode))]
public class PhysicsOverlapCircleNodeBodyEditor : VisiblePortsNodeEditor { }
#endif