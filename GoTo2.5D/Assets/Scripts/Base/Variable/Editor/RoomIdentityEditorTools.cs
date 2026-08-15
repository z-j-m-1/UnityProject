#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// RoomIdentity 编辑器补齐工具（仅编辑模式可用，播放中菜单置灰）：
/// 为场景自动创建 RoomIdentity 根物体（生成稳定 roomId）并回填房间变量资产引用，
/// 让场景改名后房间变量加载 / 存档匹配不受影响。
/// </summary>
public static class RoomIdentityEditorTools
{
    private const string RoomVariableFolder = "PersistentVariables/Room/";
    private const string RoomVariableAssetFolder = "Assets/Resources/" + RoomVariableFolder;

    [MenuItem("Tools/房间/RoomIdentity 补齐（当前场景）", false, 10)]
    private static void EnsureCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
        {
            Debug.LogWarning("RoomIdentity 补齐：当前场景未保存（临时场景），请先保存场景再执行");
            return;
        }

        if (EnsureRoomIdentityInScene(scene, out string log))
        {
            EditorSceneManager.SaveScene(scene);
            Debug.Log(log + $"\n场景 '{scene.name}' 已保存。");
        }
        else
        {
            Debug.Log(log);
        }
    }

    [MenuItem("Tools/房间/RoomIdentity 补齐（Build Settings 所有场景）", false, 11)]
    private static void EnsureAllBuildScenes()
    {
        string[] scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
            .Select(s => s.path)
            .ToArray();

        if (scenePaths.Length == 0)
        {
            Debug.LogWarning("RoomIdentity 补齐：Build Settings 中没有启用的场景");
            return;
        }

        foreach (string path in scenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            if (EnsureRoomIdentityInScene(scene, out string log))
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log(log + $"\n场景 '{scene.name}' 已保存。");
            }
            else
            {
                Debug.Log(log);
            }
        }
    }

    [MenuItem("Tools/房间/RoomIdentity 补齐（当前场景）", true)]
    [MenuItem("Tools/房间/RoomIdentity 补齐（Build Settings 所有场景）", true)]
    private static bool ValidateNotPlaying()
    {
        return !Application.isPlaying;
    }

    /// <summary>补齐单个场景：确保存在 RoomIdentity（生成 roomId）并回填变量资产引用。返回是否有改动</summary>
    private static bool EnsureRoomIdentityInScene(Scene scene, out string log)
    {
        string sceneName = scene.name;
        log = "";
        bool changed = false;

        // 1. 查找本场景的 RoomIdentity（只遍历本场景根物体，避免多场景叠加时误取）
        RoomIdentity identity = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            identity = root.GetComponentInChildren<RoomIdentity>();
            if (identity != null) break;
        }

        if (identity == null)
        {
            GameObject go = new GameObject("RoomIdentity");
            SceneManager.MoveGameObjectToScene(go, scene);
            Undo.RegisterCreatedObjectUndo(go, "补齐 RoomIdentity");
            identity = Undo.AddComponent<RoomIdentity>(go);
            changed = true;
            log += $"场景 '{sceneName}'：已创建 RoomIdentity 根物体（roomId: {identity.RoomId}）。\n";
        }

        // 2. 确保变量资产存在并按场景名回填引用
        VariableBundleObject asset = identity.VariableAsset;
        if (asset == null)
        {
            asset = Resources.Load<VariableBundleObject>(RoomVariableFolder + sceneName);
        }
        if (asset == null)
        {
            asset = CreateRoomVariableAsset(sceneName);
            log += $"场景 '{sceneName}'：已创建变量资产 '{asset.name}'。\n";
        }

        if (identity.VariableAsset != asset)
        {
            Undo.RecordObject(identity, "回填房间变量资产");
            identity.SetVariableAsset(asset);
            changed = true;
            log += $"场景 '{sceneName}'：已回填变量资产引用 '{asset.name}'。\n";
        }
        else
        {
            log += $"场景 '{sceneName}'：RoomIdentity 已就绪（roomId: {identity.RoomId}，资产: {asset.name}）。\n";
        }

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
        return changed;
    }

    /// <summary>按场景名创建房间变量资产（Assets/Resources/PersistentVariables/Room/场景名.asset）</summary>
    private static VariableBundleObject CreateRoomVariableAsset(string sceneName)
    {
        string assetPath = RoomVariableAssetFolder + sceneName + ".asset";
        string directory = Path.GetDirectoryName(assetPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        VariableBundleObject asset = ScriptableObject.CreateInstance<VariableBundleObject>();
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return asset;
    }
}
#endif
