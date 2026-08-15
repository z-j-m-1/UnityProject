# 节点图系统

基于 xNode 的可视化脚本系统。节点图资产（`BaseNodeGraph`）自带变量包（`VariableBundle`）与稳定图 GUID，可用节点读写。

## 目录结构

```
xNode/
├── BaseNodeGraph.cs       # 图资产（变量包 + 图GUID + TryGetVariable/TrySetVariable）
├── GraphExecutor.cs       # 挂在场景物体上执行图
└── Nodes/
    ├── BaseNode/          # BaseNode / DataNode / FlowNode / StartNode / EndNode
    ├── BranchNodes/       # 分支（Branch / MultiBranch / StringCondition）
    ├── LogicNodes/        # 逻辑（And / Or / No）
    ├── OrderNodes/        # 流程（Print）
    ├── ValueNodes/        # 常量值（Bool / Int / String）
    ├── TransformNodes/    # 物体变换（Move / Rote）
    ├── CommunicationNodes/# 通讯（GraphCommunicator + 事件 + 执行图 + 存档点）
    ├── UICommunicatorNodes# UI 通讯（ComUIGetTextNode / ComUISetTextNode）
    └── VariableNodes/     # 统一 get/set 变量节点（source 枚举）
```

## 统一 get/set 变量节点

一个获取基类（`GetVariableNode<T>`）+ 一个设置基类（`SetVariableNode<T>`），通过 **`VariableSource` 枚举**选择操作对象：

| source | 含义 |
|---|---|
| `Self` | 本图变量 |
| `ExternalGraph` | 跨图通讯（按目标图名） |
| `Room` / `Global` | 房间 / 全局持久变量 |

- 端口保持强类型（`T`）；加新类型只需新建 2 个节点文件（Get + Set）；
- 节点面板显示 `variableGuid`（自动记录，方便调试）；**名字优先 + GUID 兜底**解析，命中后自动回填修正；
- 具体节点：`GetVariableBool/Int/Float/StringNode`、`SetVariableBool/Int/Float/StringNode`。

## 其他节点分类

| 分类 | 节点 | 说明 |
|---|---|---|
| UI 通讯 | `ComUIGetTextNode` / `ComUISetTextNode` | 读/写 Text / TextMeshPro（source：自身或 Canvas） |
| 执行图 | `ComExecutionGraphNode` | 触发另一张图执行 |
| 存档点 | `ComSaveGameNode` | 把预备存档提交为正式存档 |
| 分支/逻辑/值/变换 | `BranchNode`、`AndLogicNode`、`BoolValueNode`、`MoveObjectNode` 等 | 流程控制、常量、物体运动 |

## 执行流程（GraphExecutor）

1. `Awake`：绑定图到挂载物体、注册到 `GraphCommunicator`；
2. `Start`：`autoExecute` 时启动协程；
3. 每 `executeInterval` 秒从 `startNode` 沿 `GetConnectedNode()` 走链式执行（上限 100 次防死循环）；
4. `executeCount`（0 = 无限循环）；`[ContextMenu("执行节点图")]` 可手动触发。

## 节点图 GUID（存档键）

- 每张图有稳定 `GUID`（`BaseNodeGraph.Guid`），**存档以图 GUID 为键**；
- 新图首次保存后持久化；`[ContextMenu("重新生成GUID")]` 可手动更换（**会破坏存档对应关系**）。

## 事件支撑

通讯 / 持久变量节点通过 `ParameterizedEvent` 泛型事件与 `GraphCommunicator`、`PersistentVariableManager` 交互（事件类集中在 `CommunicationNodes/`：`ComSetAndGetVariableEvent`、`PersistentVariableEvent`）；`GraphCommunicator` 启动时自动读档。

## 扩展模式

新节点 = 继承基类 + `[CreateNodeMenu]` 特性：

```csharp
[CreateNodeMenu("变量操作/获取/浮点")]
public class GetVariableFloatNode : GetVariableNode<float> { }
```
