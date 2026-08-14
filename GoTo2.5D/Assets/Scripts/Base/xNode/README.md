# 节点图系统

基于 xNode 的可视化脚本系统。节点图资产（`BaseNodeGraph`）自带变量包（`VariableBundle`），可用节点读写。

## 节点分类与菜单

| 分类 | 菜单 | 说明 |
|---|---|---|
| 本图变量 | `变量操作/获取|设置/字符串|布尔|整数|浮点` | 读写本图的变量包 |
| 跨图通讯 | `通讯/获取|设置/字符串|布尔|整数|浮点` | 按目标图名跨图读写变量 |
| 持久变量 | `通讯/持久变量/获取|设置/字符串|布尔|整数|浮点` | 房间 / 全局作用域（枚举选择） |
| 执行图 | `通讯/执行节点图` | 触发另一张图执行 |
| 存档点 | `通讯/存档/保存游戏` | 把预备存档提交为正式存档 |

## 结构

```
xNode/
├── BaseNodeGraph.cs              # 图资产（变量包 + GUID）
├── GraphExecutor.cs              # 挂在场景物体上执行图
└── Nodes/
    ├── BaseNode/                 # DataNode / FlowNode / BaseNode
    ├── VariableNodes/            # 本图变量 get/set
    ├── CommunicationNodes/       # 跨图通讯 + 持久变量 + 存档点 + UI 通讯
    └── ...
```

## 节点图 GUID

- 每张图有稳定 `GUID`（`BaseNodeGraph.Guid`），存档以 GUID 为键；
- 新图首次保存后持久化；`[ContextMenu("重新生成GUID")]` 可手动更换（会破坏存档对应关系）。

## 事件支撑

通讯 / 持久变量节点通过 `ParameterizedEvent` 泛型事件与 `GraphCommunicator`、`PersistentVariableManager` 交互；`GraphCommunicator` 启动时自动读档。
