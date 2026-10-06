using System;

/// <summary>
/// 场景自动创建特性：标记的单例 MonoBehaviour 会在新建场景时由编辑器自动补齐
/// （创建与脚本同名的根物体并挂载组件，已存在则跳过）。
/// 若为全局单例（DontDestroyOnLoad）则需自带 Awake 去重（销毁重复实例），以便在多场景中放置安全；RoomIdentity 等每场景组件无需去重。
/// 相关工具：SceneAutoCreateTools（Tools/房间/自动补齐场景管理器）。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class SceneAutoCreateAttribute : Attribute
{
}
