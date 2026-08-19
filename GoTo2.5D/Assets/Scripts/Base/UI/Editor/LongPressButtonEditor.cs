#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace UnityEditor.UI
{
    /// <summary>
    /// LongPressButton 检查器编辑器：uGUI 的 ButtonEditor（[CustomEditor(typeof(Button), true)]）
    /// 会隐藏子类新增字段，这里在 Button 标准字段 + onClick 之后补画长按相关字段（On Long Press 事件槽）。
    /// </summary>
    [CustomEditor(typeof(UnityEngine.UI.LongPressButton), true)]
    public class LongPressButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();   // Button 标准字段 + onClick

            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnLongPress"), new GUIContent("On Long Press"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnPointerDown"), new GUIContent("On Pointer Down"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnPointerUp"), new GUIContent("On Pointer Up"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnPointerEnter"), new GUIContent("On Pointer Enter"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_OnPointerExit"), new GUIContent("On Pointer Exit"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LongPressDuration"), new GUIContent("Long Press Duration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RepeatInterval"), new GUIContent("Repeat Interval"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SuppressClickAfterLongPress"), new GUIContent("Suppress Click After Long Press"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif