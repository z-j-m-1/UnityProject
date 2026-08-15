# GoTo2.5D

2.5D 俯视角游戏工程（Unity 2022.3.62f3c1），以**可视化节点图（xNode）驱动玩法逻辑**：玩法逻辑在节点图资产里拖出来，C# 侧提供事件总线、变量存储、存档、执行器、UI 收集等地基。

## 架构总览

```
事件系统(Event)        ← 泛型事件总线，支撑通讯/持久变量节点
变量系统(Variable)     ← 数据地基：节点图/房间/全局变量、物品状态共用
    └─ 存档系统(Save)  → 两层存档（staging/archive），持久化变量与物品
节点图系统(xNode)      ← 可视化玩法逻辑（变量读写/跨图通讯/持久变量/UI/存档点）
```

## 系统索引

| 系统 | 位置 | 文档 |
|---|---|---|
| 变量系统 | `GoTo2.5D/Assets/Scripts/Base/Variable` | [README](GoTo2.5D/Assets/Scripts/Base/Variable/README.md) |
| 存档系统 | `GoTo2.5D/Assets/Scripts/Base/Save` | [README](GoTo2.5D/Assets/Scripts/Base/Save/README.md) |
| 事件系统 | `GoTo2.5D/Assets/Scripts/Base/Event` | [README](GoTo2.5D/Assets/Scripts/Base/Event/README.md) |
| 音乐系统 | `GoTo2.5D/Assets/Scripts/Base/Music` | [README](GoTo2.5D/Assets/Scripts/Base/Music/README.md) |
| UI 系统 | `GoTo2.5D/Assets/Scripts/Base/UI` + `Base/xNode/Nodes/UICommunicatorNodes` | [README](GoTo2.5D/Assets/Scripts/Base/UI/README.md) |
| 节点图系统 | `GoTo2.5D/Assets/Scripts/Base/xNode` | [README](GoTo2.5D/Assets/Scripts/Base/xNode/README.md) |

完整架构见 [架构说明.md](架构说明.md)。

## 关键设计点

- **抗改名**：变量（Variable GUID）、图（图 GUID）、房间（roomId）都有稳定 ID，全模式**名字优先 + GUID 兜底**；
- **RoomIdentity**：场景身份组件 + 编辑器一键补齐工具（`Tools/房间/RoomIdentity 补齐`）；
- **存档 v2**：变量条目含 GUID，旧档不兼容，可直接删除 `save/` 目录重置；
- **统一 get/set 节点**：`VariableSource` 枚举 + 泛型基类，加新变量类型只需 2 个文件；
- **UICollector**：按物体名索引的 UI 收集器，节点图可直接读写 UI 文本。

## 更新记录

见 [更新说明.txt](GoTo2.5D/更新说明.txt)
