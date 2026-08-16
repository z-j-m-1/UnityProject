# 节点图系统

基于 xNode 的可视化脚本系统。节点图资产（`BaseNodeGraph`）自带变量包（`VariableBundle`）与稳定图 GUID，可用节点读写。

## 目录结构

```
xNode/
├── BaseNodeGraph.cs       # 图资产（变量包 + 图GUID + TryGetVariable/TrySetVariable）
├── GraphExecutor.cs       # 挂在场景物体上执行图
├── GraphStateMachine.cs   # 挂在场景物体上的图状态机容器（状态=子图）
├── SceneObjectFinder.cs   # 全场景物体按名字缓存查找（GetGameObjectNode All 来源用）
└── Nodes/
    ├── BaseNode/          # BaseNode / DataNode / FlowNode / StartNode / EndNode / EntryNode
    ├── BranchNodes/       # 分支（Branch / MultiBranch / StringCondition）
    ├── LogicNodes/        # 逻辑（And / Or / No）+ 比较（Compare）
    ├── MathNodes/         # 数学运算（四则运算 / 比较）
    ├── OrderNodes/        # 流程（Print / Wait / 等待条件）
    ├── StringNodes/       # 字符串运算（运算 / 比较 / 长度）
    ├── ValueNodes/        # 取值：Constants/（常量 Bool/Int/String/Vector3）+ Conversion/（类型转换 + 三浮点合成三维向量）
    ├── ObjectNodes/       # 物体引用（获取物体：Self/All 来源）
    ├── StateMachineNodes/ # 状态机（切换状态节点）
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
- 基类带 **GameObject 输入端口**（`targetGameObject`，非序列化，不显示值框）：**已连线优先取输入值**，取到 null 或未连线才回退上述目标模式 → 旧图（只配目标模式）零影响；新图可接「取值/获取物体」节点动态指定目标；
- 自动 `GetComponent<T>`，找不到给出警告；
- 子类只需实现 `Apply(T component)` 做具体动作；
- 加新操作（缩放 / 音频 / 动画等）= 继承基类 + 一个 `Apply`；
- "图绑定物体" = **当前执行器对象**（多执行器跑同一张图各自解析自己的目标，不共享）。

## 物体引用（GetGameObjectNode + SceneObjectFinder）

菜单 **取值/获取物体**，输出 `GameObject` 数据端口，可接线到任意 `ComponentActionNode` 的 GameObject 输入端口（或经子图 GameObject 参数传入子图）：

- **来源**：`Self`（图绑定物体自身 / 子物体，`transform.Find` 层级查找）/ `All`（全场景按名字查找，含 inactive）；
- **对象名称**为 `string` 输入端口（可接线，未接线用字段值）；
- 输出字段非序列化（运行时求值，规避场景引用写进图资产 / 跨场景重载失效）；
- `All` 用 `SceneObjectFinder` 缓存字典：惰性构建「名字 → 物体」索引，查找 O(1)；场景加载/卸载、运行时启动、编辑器 Hierarchy 变更自动失效；未命中重扫一次兜底；重名只取第一个并警告一次。

## 图状态机（GraphStateMachine）

把"状态 = 一张子图"的状态机挂在场景物体上（复用 GraphChainRunner + 子图机制，不依赖 GraphExecutor）：

- **状态列表** `List<GraphState>`：`stateName` + 状态子图 + `entryIdentifier`（入口标识，空 = 子图默认起点）+ `loop`（链跑完是否循环重跑）；
- **切换**：`TransitionTo(stateName)`（C# / UnityEvent 调用，或图内用菜单 **状态机/切换** 节点）；
- 切换语义：**停当前链 + 起新链**；链执行时宿主 = 状态机自身 → 子图/操作节点的 Attached 目标 = 状态机物体；
- **事件驱动（可选）**：`subscribeEntries` 开启后订阅当前状态子图的入口事件，命中即从该入口重跑当前状态链；
- `initialState` 非空时 `Start` 自动进入。

「状态机/切换」节点：`machineName`（空 = 图绑定物体上查找）/ `targetState` 均为可接线的 string 输入端口。

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

## 编辑器工具

- **节点浏览器**：`Tools/节点系统/节点浏览器`，或节点图空白处右键 →「打开节点浏览器…」。支持**搜索**（按菜单路径/类名）、**最近使用**（自动记录最近创建的 12 种节点）、**收藏**（★ 标记，EditorPrefs 持久化）。节点图空白处右键菜单顶部也会显示「★ 收藏」「最近使用」快捷项与浏览器入口；
- **变量引用下拉**：Get/Set 变量节点的节点体里，「变量名」提供**图中已定义变量**下拉（`BaseNodeGraph.GetAllVariableNames()` 扫描 VariableBundle 全类型去重），未命中保留手动输入；
- 已有：入口下拉、触发器 eventId 下拉、日志级别菜单、多链运行高亮、变量 GUID 自动回填。

## 子图封装（SubGraphNode）

把一段逻辑封装成另一张节点图，在父图里当一个节点调用（菜单 **子图/执行**）：

- **子图准备**：子图 = 普通 `BaseNodeGraph`；用「子图/参数输入」「子图/参数输出」节点声明出入口参数（**参数名 = 子图变量名**，图中唯一）；入口用 StartNode 或 EntryNode，EndNode 收尾；
- **参数端口**：`SubGraphNode` 选好子图后**自动生成与参数节点一一对应的输入/输出端口**（端口名 = 参数名），连线即传参。执行时：父图连线值 → 子图变量 → 跑子图链 → 输出节点输入求值 → 输出端口；
- **GameObject 参数**：「子图/参数输入/物体」「子图/参数输出/物体」与 5 种基础类型参数并列；**不走变量系统**（GameObject 不入 VariableBundle 序列化）——输入节点由父图执行前直接写入字段，输出节点由父图求值读回，端口同步/嵌套/循环校验逻辑完全复用；
- **子图内部接法**：**参数输入节点 = 取值源**（输出端口，连到需要参数的地方，未注入时用节点字段默认值）；**参数输出节点 = 返回值槽**（输入端口，把结果连进来，链跑完后父图读回）；
- **执行语义**：同步阻塞（父链等子图跑完，与 Wait 节点一致）；目标解析沿用父执行器（Attached 目标 = 父执行器物体）；嵌套深度上限 8（运行时拦截），编辑器做循环引用与参数重名校验；
- **注意**：同一子图被多条链并发调用时**变量共享**（要隔离就复制子图资产）；`resetVariablesOnCall` 可让每次调用重置子图变量；参数改名会断开对应端口连线。

## 扩展模式

新节点 = 继承基类 + `[CreateNodeMenu]` 特性：

```csharp
[CreateNodeMenu("变量操作/获取/浮点")]
public class GetVariableFloatNode : GetVariableNode<float> { }
```
