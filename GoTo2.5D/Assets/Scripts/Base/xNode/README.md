# 节点图系统

基于 xNode 的可视化脚本系统。节点图资产（`BaseNodeGraph`）自带变量包（`VariableBundle`）与稳定图 GUID，可用节点读写。

## 目录结构

```
xNode/
├── BaseNodeGraph.cs       # 图资产（变量包 + 图GUID + TryGetVariable/TrySetVariable）
├── GraphExecutor.cs       # 挂在场景物体上执行图
└── Nodes/
    ├── BaseNode/          # BaseNode / DataNode / FlowNode / StartNode / EndNode / EntryNode
    ├── BranchNodes/       # 分支（Branch / MultiBranch / StringCondition）
    ├── LogicNodes/        # 逻辑（And / Or / No）+ 比较（Compare）
    ├── MathNodes/         # 数学运算（四则运算 / 比较）
    ├── OrderNodes/        # 流程（Print / Wait / 等待条件）
    ├── StringNodes/       # 字符串运算（运算 / 比较 / 长度）
    ├── ValueNodes/        # 取值：Constants/（常量 Bool/Int/String/Vector3）+ Conversion/（类型转换 + 三浮点合成三维向量）
    ├── TransformNodes/    # 物体变换（Move / Rote / Scale / SetPosition / SetRotation，继承 ComponentActionNode）
    ├── AudioNodes/        # 音频（Play / Stop，继承 ComponentActionNode）
    ├── AnimationNodes/    # 动画（Play，继承 ComponentActionNode）
    ├── CommunicationNodes/# 通讯（GraphCommunicator + 事件 + 执行图 + 存档点）
    ├── UICommunicatorNodes# UI 通讯（ComUIGetTextNode / ComUISetTextNode）
    └── VariableNodes/     # 变量操作：Get/（获取）+ Set/（设置），source 枚举选操作对象
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
- `variableGuid` / 入口节点 `guid` 只在**检查器（Inspector）**中显示，节点体上不显示（避免误触）；
- 具体节点：`GetVariableBool/Int/Float/String/Vector3Node`、`SetVariableBool/Int/Float/String/Vector3Node`。

## 其他节点分类

| 分类 | 节点 | 说明 |
|---|---|---|
| UI 通讯 | `ComUIGetTextNode` / `ComUISetTextNode` | 读/写 Text / TextMeshPro（source：自身或 Canvas） |
| 执行图 | `ComExecutionGraphNode` | 触发另一张图执行 |
| 存档点 | `ComSaveGameNode` | 把预备存档提交为正式存档 |
| 变换 | `MoveObjectNode` / `RoteObjectNode` / `ScaleObjectNode` / `SetPositionNode` / `SetRotationNode` | 移动 / 旋转 / 缩放 / 设置位置 / 设置旋转（目标：图绑定物体 / 子物体名 / 直接引用） |
| 音频/动画 | `PlayAudioNode` / `StopAudioNode` / `PlayAnimationNode` | 播放/停止音频（AudioSource）、播放动画（Animator.Play） |
| 分支/逻辑/值/变换 | `BranchNode`、`AndLogicNode`、`BoolValueNode` 等 | 流程控制、常量、物体运动 |
| 数学运算 | `MathOpIntNode` / `MathOpFloatNode` / `CompareIntNode` / `CompareFloatNode` / `RandomIntNode` / `RandomFloatNode` | 四则运算、比较、随机整数/浮点 |
| 字符串 | `StringOpNode` / `StringCompareNode` / `StringLengthNode` / `StringSubstringNode` / `StringReplaceNode` | 拼接/大小写/去空格、比较、长度、截取、替换 |
| 转换 | `IntToFloatNode` / `FloatToIntNode` / `IntToStringNode` / `FloatToStringNode` / `StringToIntNode` / `StringToFloatNode` | int↔float↔string 互转 |
| 流程 | `PrintNode` / `WaitNode` / `WaitUntilNode` | 日志输出 / 等待指定秒数 / 等待条件成立（可接比较·逻辑·变量节点，支持超时） |

## 执行流程（GraphExecutor）

> 执行器枚举、触发策略、多链并发与事件订阅的**详细说明**见 [GraphExecutor.README.md](GraphExecutor.README.md)。

1. `Awake`：绑定图到挂载物体、注册到 `GraphCommunicator`；
2. `Start`：`autoExecute` 启动默认链；`entryEventSubscribe != Off` 时订阅入口事件；
3. **多链并发**：每条链是独立协程（每次事件/触发启动一条），互不打断；每链每 `executeInterval` 秒从起点沿 `GetConnectedNode()` 走链式执行（上限 100 次防死循环）；
4. `executeCount`（0 = 无限循环）按链独立计数；`[ContextMenu("执行节点图")]` 可手动触发；
5. 触发策略 `triggerPolicy` **按同一起点生效**：`Restart`（默认，重触发=停止该起点旧链并重跑）/ `IgnoreWhileRunning`（运行中忽略）/ `Queue`（运行中排队，当前链跑完自动再跑一轮）；
6. 执行游标为**执行器私有**，多个执行器跑同一张图互不干扰；共享图变量（并发链合作/独立按变量划分）；

## 组件动作节点（ComponentActionNode）

统一"目标解析 + 组件获取"的泛型基类 `ComponentActionNode<T>`：

- 目标三模式：`Attached`（图绑定物体）/ `ByName`（子物体名查找）/ `Direct`（直接拖引用）；
- 自动 `GetComponent<T>`，找不到给出警告；
- 子类只需实现 `Apply(T component)` 做具体动作；
- 加新操作（缩放 / 音频 / 动画等）= 继承基类 + 一个 `Apply`；
- "图绑定物体" = **当前执行器对象**（多执行器跑同一张图各自解析自己的目标，不共享）。

## 日志级别

`NodeLog` 统一日志工具（`Error/Warning/Info/Verbose` 分级，默认 `Warning`）。
菜单 **Tools/节点系统/日志级别** 可切换 Info / Verbose 查看详细运行日志（变量读写、节点执行、图通讯等）。

## 入口节点（EntryNode）

一张图可放多个入口节点（`基本/入口`），各自带标识符 + 自动 GUID。

- `GraphExecutor` 执行模式：`Default`（默认从 `startNode`）/ `Entry`（按标识符或 GUID 从入口节点开始执行）；
- 入口模式下未找到对应入口 → `LogError` 且**不执行**（不回退 startNode）；
- `BaseNodeGraph.GetEntryNode(id)` 运行时 / 编辑器都实时扫描 `nodes`，动态变更也能命中；
- Inspector 里入口模式下提供下拉选择器；标识符改名后会自动回填修正（编辑模式）。

## 节点图 GUID（存档键）

- 每张图有稳定 `GUID`（`BaseNodeGraph.Guid`），**存档以图 GUID 为键**；
- 新图首次保存后持久化；`[ContextMenu("重新生成GUID")]` 可手动更换（**会破坏存档对应关系**）。

## 事件支撑

通讯 / 持久变量节点通过 `ParameterizedEvent` 泛型事件与 `GraphCommunicator`、`PersistentVariableManager` 交互（事件类集中在 `CommunicationNodes/`：`ComSetAndGetVariableEvent`、`PersistentVariableEvent`）；`GraphCommunicator` 启动时自动读档。

- **触发器入口标识下拉**：`GraphEventEmitter` / `CollisionEventEmitter` 的 Inspector 会扫描场景中所有执行器使用的节点图及其入口，提供 eventId 下拉（避免手填拼错）；选中后回填入口标识符，未命中显示警告，仍可手动输入。复用：自定义触发器编辑器调用 `GraphEventEntryOptionPicker.CollectEntryOptions()` + `DrawEventIdPicker()`。

## 扩展模式

新节点 = 继承基类 + `[CreateNodeMenu]` 特性：

```csharp
[CreateNodeMenu("变量操作/获取/浮点")]
public class GetVariableFloatNode : GetVariableNode<float> { }
```
