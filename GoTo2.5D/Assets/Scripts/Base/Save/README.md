# 存档系统

节点图变量、房间变量、全局变量、物品状态的持久化。

## 两层存档

- **staging（预备）**：离开房间时自动写入，保证房间进度不丢
- **archive（正式）**：真正保存 = staging 覆盖 archive；**读档只读 archive**

```
persistentDataPath/save/
├── index.json          # lastScene、saveTime
├── global.json         # 全局变量（仅真正保存时写）
├── staging/<房间>.json # 预备
└── archive/<房间>.json # 正式
```

## 核心类

| 类 | 说明 |
|---|---|
| `RoomSaveData` | 单房间存档：局部变量 + 图变量（按图GUID）+ 物品状态 |
| `GlobalSaveData` | 全局变量 |
| `SaveIndex` | 存档元数据 |
| `SaveSystem` | 场景单例：`Commit` / `Load` / `Delete` / `ApplyRoomSave` |
| `PersistentItem` | 物品基类（持有 VariableBundle + `OnBeforeSave`/`OnAfterLoad` 钩子） |
| `PersistentItemManager` | 物品状态收集/分发单例 |

## API

```csharp
SaveSystem.Commit();   // 真正保存（存档点 / Inspector 右键）
SaveSystem.Load();     // 读档（启动自动调用）
SaveSystem.Delete();   // 删除整个 save 目录
SaveSystem.HasSave();  // 是否存在正式存档
```

## 物品持久化

物品挂 `PersistentItem`，状态直接存 `Variables`（VariableBundle），移除逻辑由物品自己处理：

```csharp
public class MyItem : PersistentItem
{
    public override void OnAfterLoad()
    {
        if (Variables.Get<bool>("removed", false)) gameObject.SetActive(false);
    }

    public void Pickup()
    {
        Variables.Set("removed", true);
        gameObject.SetActive(false);
    }
}
```

## 注意事项

- 存档文件位置：`C:\Users\<用户名>\AppData\LocalLow\DefaultCompany\GoTo2.5D\save\`
- 房间文件按场景分槽，只有"进入过并保存过"的房间才有存档
- `itemId` / 图 GUID 生成后需在编辑器保存一次场景/资产才会持久化
