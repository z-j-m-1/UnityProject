#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景自动补齐工具（仅编辑模式，播放中菜单置灰）：
/// 1. 通用 provisioner：扫描所有带 [SceneAutoCreate] 特性的组件，为场景创建与脚本同名的根物体并挂载组件（已存在跳过）；
/// 2. RoomIdentity 特例：确保房间场景有 RoomIdentity（生成稳定 roomId）并回填房间变量资产引用。
/// 新建场景时自动执行通用 provisioner（EditorSceneManager.newSceneCreated）。
/// 菜单：Tools/房间/自动补齐（当前场景 / Build Settings 所有场景）
/// </summary>
public static class SceneProvisionTools
{
    private const string RoomVariableFolder = "PersistentVariables/Room/";
    private const string RoomVariableAssetFolder = "Assets/Resources/" + RoomVariableFolder;

    private static readonly List<Type> cachedAutoCreateTypes = new List<Type>();
    private static bool registered;

    [InitializeOnLoadMethod]
    private static void RegisterAutoProvisionHook()
    {
        if (registered) return;
        registered = true;
        EditorSceneManager.newSceneCreated += (scene, setup, mode) =>
        {
            int n = ProvisionScene(scene);
            if (n > 0) EditorSceneManager.MarkSceneDirty(scene);
        };
    }

    // ============ 菜单 ============

    [MenuItem("Tools/房间/自动补齐（当前场景）", false, 10)]
    private static void EnsureCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
        {
            Debug.LogWarning("自动补齐：当前场景未保存（临时场景），请先保存场景再执行");
            return;
        }

        int managers = ProvisionScene(scene);
        bool changed = EnsureRoomIdentityInScene(scene, out string log);
        if (changed || managers > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
        Debug.Log(log + $"\n场景 '{scene.name}'：已补齐管理器 {managers} 个。");
    }

    [MenuItem("Tools/房间/自动补齐（Build Settings 所有场景）", false, 11)]
    private static void EnsureAllBuildScenes()
    {
        string[] validPaths = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path) && File.Exists(s.path))
            .Select(s => s.path)
            .ToArray();

        string[] missingPaths = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path) && !File.Exists(s.path))
            .Select(s => s.path)
            .ToArray();

        if (missingPaths.Length > 0)
        {
            Debug.LogWarning("自动补齐：以下 Build Settings 场景文件不存在，已跳过（可在 Build Settings 中移除失效条目）：\n" + string.Join("\n", missingPaths));
        }

        if (validPaths.Length == 0)
        {
            Debug.LogWarning("自动补齐：Build Settings 中没有可处理的场景");
            return;
        }

        foreach (string path in validPaths)
        {
            try
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int managers = ProvisionScene(scene);
                bool changed = EnsureRoomIdentityInScene(scene, out string log);
                if (changed || managers > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                }
                Debug.Log(log + $"\n场景 '{scene.name}'：已补齐管理器 {managers} 个。");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"自动补齐：处理场景 '{path}' 失败，已跳过（{e.Message}）");
            }
        }
    }

    [MenuItem("Tools/房间/自动补齐（当前场景）", true)]
    [MenuItem("Tools/房间/自动补齐（Build Settings 所有场景）", true)]
    private static bool ValidateNotPlaying()
    {
        return !Application.isPlaying;
    }

    // ============ 通用 provisioner（[SceneAutoCreate] 特性） ============

    /// <summary>为场景补齐所有带 [SceneAutoCreate] 的组件（创建与脚本同名的根物体并挂载组件，已存在跳过）。返回新增数量</summary>
    public static int ProvisionScene(Scene scene)
    {
        if (!scene.IsValid()) return 0;

        int created = 0;
        foreach (Type type in CollectAutoCreateTypes())
        {
            if (HasComponentInScene(scene, type)) continue;

            GameObject go = new GameObject(type.Name);
            SceneManager.MoveGameObjectToScene(go, scene);
            Undo.RegisterCreatedObjectUndo(go, "自动补齐 " + type.Name);
            Undo.AddComponent(go, type);
            created++;
        }
        return created;
    }

    private static List<Type> CollectAutoCreateTypes()
    {
        if (cachedAutoCreateTypes.Count == 0)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.GetName().Name.StartsWith("Assembly-CSharp")) continue;
                foreach (Type t in asm.GetTypes())
                {
                    if (!typeof(MonoBehaviour).IsAssignableFrom(t)) continue;
                    if (t.IsDefined(typeof(SceneAutoCreateAttribute), false))
                    {
                        cachedAutoCreateTypes.Add(t);
                    }
                }
            }
        }
        return cachedAutoCreateTypes;
    }

    private static bool HasComponentInScene(Scene scene, Type type)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.GetComponentInChildren(type) != null) return true;
        }
        return false;
    }
    // ============ RoomIdentity + 房间变量资产 ============

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