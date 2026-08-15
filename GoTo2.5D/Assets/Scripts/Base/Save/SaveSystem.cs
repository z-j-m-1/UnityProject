using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameFrameX.LitJSON.Runtime;

/// <summary>
/// 存档系统（场景单例）
/// 两层存档：staging（离开房间自动写的预备文件）→ archive（真正保存 = 提交，读档只读这一层）
/// 房间文件包含：房间局部变量 + 该房间各图变量（按图GUID）+ 物品状态（后续扩展）
/// </summary>
public class SaveSystem : MonoBehaviour
{
    private const string SaveRootFolder = "save";
    private const string StagingFolder = "staging";
    private const string ArchiveFolder = "archive";
    private const string IndexFileName = "index.json";
    private const string GlobalFileName = "global.json";
    private const int CurrentVersion = 2;

    private static SaveSystem _instance;

    /// <summary>
    /// 存档系统单例（不存在则自动创建）
    /// </summary>
    public static SaveSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveSystem>();
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(SaveSystem).Name);
                    _instance = go.AddComponent<SaveSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    /// <summary>存档目录完整路径</summary>
    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveRootFolder);

    private static string StagingPath => Path.Combine(SavePath, StagingFolder);
    private static string ArchivePath => Path.Combine(SavePath, ArchiveFolder);
    private static string IndexPath => Path.Combine(SavePath, IndexFileName);
    private static string GlobalPath => Path.Combine(SavePath, GlobalFileName);
    private static string StagingRoomPath(string sceneName) => Path.Combine(StagingPath, sceneName + ".json");
    private static string ArchiveRoomPath(string sceneName) => Path.Combine(ArchivePath, sceneName + ".json");

    /// <summary>是否存在存档（archive 层有房间文件）</summary>
    public static bool HasSave()
    {
        return Directory.Exists(ArchivePath) && Directory.GetFiles(ArchivePath, "*.json").Length > 0;
    }

    // ============ 静态入口（内部转调实例，保持原有调用方式） ============

    /// <summary>真正保存：把预备文件提交为正式存档</summary>
    public static void Commit() => Instance.CommitNow();
    public static void Load() => Instance.LoadNow();
    public static void Delete() => Instance.DeleteNow();

    /// <summary>应用某个房间的存档（进入房间或加载存档时调用）</summary>
    public static void ApplyRoomSave(string sceneName) => Instance.ApplyRoomSaveNow(sceneName);

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 离开房间时自动把该房间状态写入预备文件
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // 离开房间：自动保存预备文件（不丢房间进度）
    private void OnSceneUnloaded(Scene scene)
    {
        if (!RoomVariableManager.IsInitialized) return;
        WriteStagingRoom(scene.name, CaptureRoom(scene.name));
        Debug.Log($"存档系统：离开房间 '{scene.name}'，已写入预备存档");
    }

    // ============ 实例方法（Inspector 右键菜单） ============

    /// <summary>
    /// 真正保存：当前房间入预备 → 预备覆盖存档 → 写全局/索引（仅运行时可调用）
    /// </summary>
    [ContextMenu("保存存档")]
    public void CommitNow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("存档系统：请先在运行状态（Play）下保存存档");
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;

        // 1. 先把当前房间状态写入预备文件（当前房间还没离开，不会被自动保存覆盖）
        WriteStagingRoom(sceneName, CaptureRoom(sceneName));

        // 2. 预备文件 → 存档文件（逐文件原子复制）
        CommitStagingToArchive();

        // 提交完成后删除预备文件（staging 是临时保护层，正式保存后清空）
        ClearStaging();

        // 3. 全局变量 + 索引（global.json 只在真正保存时写）
        WriteGlobal();
        WriteIndex(sceneName);

        Debug.Log($"存档系统：已保存存档到 '{SavePath}'");
    }

    /// <summary>
    /// 加载存档：导入全局变量 + 当前房间存档（只读 archive）
    /// </summary>
    [ContextMenu("加载存档")]
    public void LoadNow()
    {
        GlobalSaveData globalData = ReadJson<GlobalSaveData>(GlobalPath);
        if (globalData != null && globalData.global != null)
        {
            GameGlobalVariableManager.Instance.Import(globalData.global);
        }

        SaveIndex index = ReadJson<SaveIndex>(IndexPath);
        string sceneName = SceneManager.GetActiveScene().name;
        ApplyRoomSaveNow(sceneName);

        Debug.Log($"存档系统：已加载存档（场景 '{sceneName}'，存档场景 '{index?.lastScene ?? "无"}'）");
    }

    /// <summary>
    /// 删除存档（整个 save 目录）
    /// </summary>
    [ContextMenu("删除存档")]
    public void DeleteNow()
    {
        if (Directory.Exists(SavePath))
        {
            Directory.Delete(SavePath, true);
            Debug.Log("存档系统：已删除存档");
        }
    }

    // ============ 私有辅助 ============

    /// <summary>
    /// 应用某个房间的存档（只读 archive）。进入房间或加载存档时调用。
    /// </summary>
    public void ApplyRoomSaveNow(string sceneName)
    {
        // 确保房间管理器存在并已加载该房间对象、登记该房间图
        RoomVariableManager roomManager = RoomVariableManager.Instance;
        roomManager.RegisterCurrentRoomGraphs(sceneName);

        RoomSaveData roomData = ReadJson<RoomSaveData>(ArchiveRoomPath(sceneName));
        string oldSaveFile = null;

        // 场景改名后按场景名找不到文件：用房间ID扫描 archive 兜底
        if (roomData == null)
        {
            string roomId = roomManager.GetRoomId(sceneName);
            roomData = FindRoomSaveByRoomId(roomId, out oldSaveFile);
            if (roomData != null)
            {
                Debug.Log($"存档系统：按场景名 '{sceneName}' 未找到存档，已按房间ID '{roomId}' 匹配到存档");
            }
        }

        if (roomData == null)
        {
            Debug.Log($"存档系统：房间 '{sceneName}' 无存档，使用默认值");
            return;
        }

        roomManager.Import(roomData.localVariables);
        roomManager.ImportRoomGraphs(sceneName, roomData.graphs);

        // 物品状态还原
        if (PersistentItemManager.IsInitialized && roomData.items != null)
        {
            PersistentItemManager.Instance.ApplyCurrent(roomData.items);
        }

        // 场景改名兜底命中后：删除旧名称的存档与预备文件
        if (oldSaveFile != null)
        {
            DeleteOldRoomFiles(oldSaveFile, sceneName);
        }
    }

    /// <summary>
    /// 在 archive 层扫描某个房间ID对应的存档（场景改名后的兜底匹配）
    /// </summary>
    private static RoomSaveData FindRoomSaveByRoomId(string roomId, out string filePath)
    {
        filePath = null;
        if (string.IsNullOrEmpty(roomId) || !Directory.Exists(ArchivePath)) return null;
        foreach (string file in Directory.GetFiles(ArchivePath, "*.json"))
        {
            RoomSaveData data = ReadJson<RoomSaveData>(file);
            if (data != null && !string.IsNullOrEmpty(data.roomId) && data.roomId == roomId)
            {
                filePath = file;
                return data;
            }
        }
        return null;
    }

    /// <summary>
    /// 采集某个房间的当前状态（局部变量 + 图变量 + 物品）
    /// </summary>
    private RoomSaveData CaptureRoom(string sceneName)
    {
        return new RoomSaveData
        {
            version = CurrentVersion,
            sceneName = sceneName,
            roomId = RoomVariableManager.Instance.GetRoomId(sceneName),
            localVariables = RoomVariableManager.Instance.Export(),
            graphs = RoomVariableManager.Instance.ExportRoomGraphs(sceneName),
            items = PersistentItemManager.IsInitialized ? PersistentItemManager.Instance.ExportCurrent() : null
        };
    }

    /// <summary>
    /// 把房间数据写入预备文件（原子：先写 .tmp 再改名）
    /// </summary>
    private void WriteStagingRoom(string sceneName, RoomSaveData data)
    {
        Directory.CreateDirectory(StagingPath);
        string json = JsonMapper.ToJson(data, true);
        string tmp = StagingRoomPath(sceneName) + ".tmp";
        File.WriteAllText(tmp, json);
        MoveFile(tmp, StagingRoomPath(sceneName));
    }

    /// <summary>
    /// 移动文件并覆盖已存在目标（兼容无 overwrite 重载的 .NET API）
    /// </summary>
    private static void MoveFile(string source, string dest)
    {
        if (File.Exists(dest))
        {
            File.Delete(dest);
        }
        File.Move(source, dest);
    }

    /// <summary>
    /// 预备文件 → 存档文件（逐文件原子复制）
    /// </summary>
    private void CommitStagingToArchive()
    {
        if (!Directory.Exists(StagingPath)) return;
        Directory.CreateDirectory(ArchivePath);
        foreach (string file in Directory.GetFiles(StagingPath, "*.json"))
        {
            string fileName = Path.GetFileName(file);
            string dest = Path.Combine(ArchivePath, fileName);
            string tmp = dest + ".tmp";
            File.Copy(file, tmp, true);
            MoveFile(tmp, dest);
        }
    }

    /// <summary>
    /// 写全局变量文件（仅真正保存时调用）
    /// </summary>
    /// <summary>
    /// 提交完成后删除预备层所有文件（staging 是临时保护层，正式保存后清空）
    /// </summary>
    private void ClearStaging()
    {
        if (!Directory.Exists(StagingPath)) return;
        foreach (string file in Directory.GetFiles(StagingPath, "*.json"))
        {
            try
            {
                File.Delete(file);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"存档系统：删除预备文件失败（{file}），{e.Message}");
            }
        }
    }

    /// <summary>
    /// 场景改名兜底命中后：删除旧名称的存档文件与对应的预备文件
    /// </summary>
    private void DeleteOldRoomFiles(string archiveFilePath, string newSceneName)
    {
        string oldName = Path.GetFileNameWithoutExtension(archiveFilePath);
        if (string.IsNullOrEmpty(oldName) || oldName == newSceneName) return;

        try
        {
            File.Delete(archiveFilePath);
            Debug.Log($"存档系统：已删除旧房间名存档 '{archiveFilePath}'");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"存档系统：删除旧存档失败（{archiveFilePath}），{e.Message}");
        }

        string oldStaging = StagingRoomPath(oldName);
        if (File.Exists(oldStaging))
        {
            try
            {
                File.Delete(oldStaging);
                Debug.Log($"存档系统：已删除旧房间名预备文件 '{oldStaging}'");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"存档系统：删除旧预备文件失败（{oldStaging}），{e.Message}");
            }
        }
    }

    private void WriteGlobal()
    {
        Directory.CreateDirectory(SavePath);
        GlobalSaveData data = new GlobalSaveData
        {
            version = CurrentVersion,
            global = GameGlobalVariableManager.Instance.Export()
        };
        File.WriteAllText(GlobalPath, JsonMapper.ToJson(data, true));
    }

    /// <summary>
    /// 写存档索引
    /// </summary>
    private void WriteIndex(string sceneName)
    {
        Directory.CreateDirectory(SavePath);
        SaveIndex index = new SaveIndex
        {
            version = CurrentVersion,
            lastScene = sceneName,
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        File.WriteAllText(IndexPath, JsonMapper.ToJson(index, true));
    }

    /// <summary>
    /// 读取 JSON 文件；不存在或解析失败返回 null
    /// </summary>
    private static T ReadJson<T>(string path) where T : class
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
        try
        {
            return JsonMapper.ToObject<T>(File.ReadAllText(path));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"存档系统：读取文件失败（{path}），{e.Message}");
            return null;
        }
    }
}
