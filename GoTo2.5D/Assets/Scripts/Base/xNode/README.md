# 节点图系统

基于 xNode 的可视化脚本系统。节点图资产（`BaseNodeGraph`）自带变量包（`VariableBundle`），可用节点读写。

## 节点分类与菜单

| 节点 | 菜单 | 说明 |
|---|---|---|
| 获取/设置变量 | `变量操作/获取\|设置/字符串\|布尔\|整数\|浮点` | 通过 `source` 枚举选择操作对象 |
| UI 文本 | `通讯UI/获取\|设置文本` | 读/写 Text / TextMeshPro（`source`：自身或 Canvas） |
| 执行图 | `通讯/执行节点图` | 触发另一张图执行 |
| 存档点 | `通讯/存档/保存游戏` | 把预备存档提交为正式存档 |

## 统一 get/set 变量节点

一个获取基类（`GetVariableNode<T>`）+ 一个设置基类（`SetVariableNode<T>`），通过 **`VariableSource` 枚举**选择操作对象：

| source | 含义 |
|---|---|
| `Self` | 本图变量 |
| `ExternalGraph` | 跨图通讯（按目标图名/物体名） |
| `Room` / `Global` | 房间 / 全局持久变量 |

- 端口保持强类型（`T`）；加新类型只需新建 2 个节点文件（Get + Set）

## 结构

```
xNode/
├── BaseNodeGraph.cs              # 图资产（变量包 + GUID）
├── GraphExecutor.cs              # 挂在场景物体上执行图
└── Nodes/
    ├── BaseNode/                 # DataNode / FlowNode / BaseNode
    ├── VariableNodes/            # 统一 get/set 节点（source 枚举）
    ├── CommunicationNodes/       # 通讯事件 + 存档点
    ├── ScopeVariableNode/        # 持久变量事件
    └── UICommunicatorNodes/      # 统一 UI 文本节点（source/type 枚举）
```

## 节点图 GUID

- 每张图有稳定 `GUID`（`BaseNodeGraph.Guid`），存档以 GUID 为键；
- 新图首次保存后持久化；`[ContextMenu("重新生成GUID")]` 可手动更换（会破坏存档对应关系）。

## 事件支撑

通讯 / 持久变量节点通过 `ParameterizedEvent` 泛型事件与 `GraphCommunicator`、`PersistentVariableManager` 交互；`GraphCommunicator` 启动时自动读档。
