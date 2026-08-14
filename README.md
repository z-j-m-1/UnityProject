# GoTo2.5D

2.5D 俯视角游戏工程（Unity）。

## 系统架构总览

```
事件系统(Event)        ← 泛型事件总线，支撑通讯/持久变量节点
变量系统(Variable)     ← 数据地基：节点图/房间/全局变量、物品状态共用
    └─ 存档系统(Save)  → 两层存档，持久化变量与物品
节点图系统(xNode)      ← 可视化玩法逻辑（本图变量/跨图通讯/持久变量/存档点）
```

## 系统索引

| 系统 | 位置 | 文档 |
|---|---|---|
| 变量系统 | `GoTo2.5D/Assets/Scripts/Base/Variable` | [README](GoTo2.5D/Assets/Scripts/Base/Variable/README.md) |
| 存档系统 | `GoTo2.5D/Assets/Scripts/Base/Save` | [README](GoTo2.5D/Assets/Scripts/Base/Save/README.md) |
| 事件系统 | `GoTo2.5D/Assets/Scripts/Base/Event` | [README](GoTo2.5D/Assets/Scripts/Base/Event/README.md) |
| 音乐系统 | `GoTo2.5D/Assets/Scripts/Base/Music` | [README](GoTo2.5D/Assets/Scripts/Base/Music/README.md) |
| UI 系统 | `GoTo2.5D/Assets/Scripts/Base/UI` + `GoTo2.5D/Assets/Scripts/Base/xNode/Nodes/UICommunicatorNodes` | [README](GoTo2.5D/Assets/Scripts/Base/UI/README.md) |
| 节点图系统 | `GoTo2.5D/Assets/Scripts/Base/xNode` | [README](GoTo2.5D/Assets/Scripts/Base/xNode/README.md) |

## 更新记录

见 [更新说明.txt](GoTo2.5D/更新说明.txt)
