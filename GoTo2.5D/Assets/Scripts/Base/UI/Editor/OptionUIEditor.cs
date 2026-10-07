#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace UnityEditor.UI
{
    /// <summary>
    /// OptionUI 检查器编辑器：uGUI 的 ButtonEditor（<c>[CustomEditor(typeof(Button), true)]</c>，
    /// <c>true</c> = 包含子类）**只画 Button/Selectable 自己的字段，会隐藏子类新增字段** ——
    /// 所以 OptionUI 的图片/文本引用在面板上看不到、也拖不进去。
    ///
    /// 这里在 Button 标准字段 + onClick 之后把 OptionUI 的字段补画出来。
    /// 写法与 <c>LongPressButtonEditor</c> 完全一致（同一个问题的既有解法）。
    /// </summary>
    [CustomEditor(typeof(OptionUI), true)]
    public class OptionUIEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();   // Button / Selectable 标准字段 + On Click

            serializedObject.Update();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("选项 UI 组件引用", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("borderImage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundImage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("optionText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iconImage"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedMark"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
