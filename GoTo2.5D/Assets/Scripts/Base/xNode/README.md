# 节点图系统

基于 xNode 的可视化脚本系统。节点图资产（`BaseNodeGraph`）自带变量包（`VariableBundle`）与稳定图 GUID，可用节点读写。

## 文档索引

- [节点系统参考.md](节点系统参考.md) — **脚本参考 + 机制速查**（逐脚本职责/关键 API + 链执行/参数/子图/状态机等机制）
- [节点清单.md](节点清单.md) — **逐节点清单**（菜单/类/基类/端口/字段/摘要，由 `Tools/节点系统/生成节点参考文档` 自动生成，勿手改）
- [GraphExecutor.README.md](GraphExecutor.README.md) — 执行器细节（枚举/触发策略/多链并发/事件订阅）

## 目录结构

```
xNode/
├── BaseNodeGraph.cs       # 图资产（变量包 + 图GUID + TryGetVariable/TrySetVariable）
├── GraphExecutor.cs       # 挂在场景物体上执行图
├── GraphStateMachine.cs   # 挂在场景物体上的图状态机容器（状态=子图）
├── GraphParams.cs         # 外部参数包（C# 触发图时携带的命名参数）
├── GraphParamList.cs      # 可序列化参数列表（外部脚本 Inspector 可视化编辑）+ GraphParamEmitter.cs
└── Nodes/
    ├── BaseNode/          # BaseNode / DataNode / FlowNode / StartNode / EndNode / EntryNode
    ├── BranchNodes/       # 分支（Branch / MultiBranch / StringCondition）
    ├── LogicNodes/        # 逻辑（And / Or / No）+ 比较（Compare）
    ├── MathNodes/         # 数学运算（四则运算 / 比较 / 二维向量运算与缩放）
    ├── ListNodes/         # 列表操作（添加 / 移除 / 取元素 / 数量 / 是否包含）
    ├── FlowControlNodes/  # 流程控制（计数/条件/遍历循环、并行、跳转入口、计时器）
    ├── OrderNodes/        # 流程（Print / Wait / 等待条件）
    ├── StringNodes/       # 字符串运算（运算 / 比较 / 长度）
    ├── ValueNodes/        # 取值：Constants/（常量 Bool/Int/Float/String/Vector2/Vector3）+ Conversion/（类型转换 + 浮点合成向量 + 向量互转）
    ├── ObjectNodes/       # 获取物体节点族（自身 / 名称 / 全场景 / 父物体 / 子物体 / 根物体，均 DataNode）
    ├── StateMachineNodes/ # 状态机（切换状态节点）
    ├── TransformNodes/    # 物体变换（Move / Rote / Scale / SetPosition / SetRotation，继承 ComponentActionNode）
    ├── AudioNodes/        # 音频动作（Stop：停止目标 AudioSource；播放音效/音乐走 ServiceNodes 的 MusicManager 单例）
    ├── ServiceNodes/      # 全局服务（走单例，不需要目标物体；如 MusicManager 播音乐/音效/调音量）
    ├── AnimationNodes/    # 动画（播放：Play / CrossFade 模式枚举 + 设置参数：TypedValue 值联合，继承 ComponentActionNode）
    ├── TweenNodes/        # 插值（移动到 / 透明度 / 材质浮点；统一继承 TweenNodeBase → 自带可编辑曲线）
    ├── CinemachineNodes/  # Cinemachine 相机（优先级切换/跟随/注视/震屏/噪声/轨道/目标组）
    ├── SpawnNodes/        # 生成/销毁（SpawnObjectNode / DestroyObjectNode）
    ├── PhysicsNodes/      # 物理查询（3D/2D 射线检测、球形/圆形检测）
    ├── RigidbodyNodes/    # 刚体控制（施加力/设置速度/角速度，3D+2D，继承 ComponentActionNode）
    ├── DialogueNodes/     # 对话UI：设置（文本 / 图片(槽位枚举) / 对话音效）+ 获取（对话内容 / 三张图片）
    ├── MaterialNodes/     # 材质属性（MaterialPropertyBlock 设置/读取；设置节点用 TypedValue 值联合）
    ├── OptionNodes/       # 选项UI（显示选项：显示 + 可选本节点内等待 + 输出选择结果，一个节点搞定）
    ├── SubGraphNodes/     # 子图执行（SubGraphNode）+ 统一参数节点（参数/输入、参数/输出）
    ├── CommunicationNodes/# 通讯（GraphCommunicator + 事件 + 执行图 + 存档点）
    ├── UICommunicatorNodes# UI 通讯（ComUIGetTextNode / ComUISetTextNode）
    ├── VariableNodes/     # 变量操作：Get/（获取）+ Set/（设置），source 枚举选操作对象；含 Vector2 与列表（List）变量节点
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
- 具体节点：`GetVariableBool/Int/Float/String/Vector2/Vector3Node`、`SetVariableBool/Int/Float/String/Vector2/Vector3Node`；
- **Vector2 / 列表变量**也走同一基类：`GetVariableVector2Node`、`GetVariableStringListNode`（字符串/整数/浮点/Vector2/Vector3 五种列表）等，source 同样支持本图/跨图/房间/全局。

## 其他节点分类

| 分类 | 节点 | 说明 |
|---|---|---|
| UI | `ImageFillNode` | 图片填充：设置（自动 Clamp 0~1）/ 读取（输出端口 `currentFill`），模式枚举切换 —— 原「UI/图片填充」`SetImageFillNode` 与「UI/图片填充值」`GetImageFillNode` 已合并删除 |
| UI 通讯 | `ComUIGetTextNode` / `ComUISetTextNode` | 读/写 Text / TextMeshPro（source：自身或 Canvas） |
| 执行图 | `ComExecutionGraphNode` | 触发另一张图执行 |
| 存档点 | `ComSaveGameNode` | 把预备存档提交为正式存档 |
| 获取物体 | `GetSelfObjectNode` / `GetNamedObjectNode` / `GetSceneObjectNode` / `GetParentObjectNode` / `GetChildObjectNode` / `GetRootObjectNode` | 目标来源拆成独立数据节点：自身（当前执行器物体）/ 名称（自身及直接子物体）/ 全场景（所有已加载场景按名查找，含未激活，**带缓存**）/ 父物体 / 子物体（任意物体的子物体，按索引或名称）/ 根物体，接到组件节点的「目标物体」端口 |
| 变换 | `TransformOpNode` | **一个节点覆盖原来的五个**（`TransformOperation` 模式枚举：`SetPosition` 设置位置 / `SetRotation` 设置旋转 / `MoveBy` 移动 / `RotateBy` 旋转 / `ScaleBy` 缩放）。各模式行为与原节点**逐行一致**；原 `SetPositionNode` / `SetRotationNode` / `MoveObjectNode` / `RoteObjectNode` / `ScaleObjectNode` 已合并删除。⚠️ 原 `ScaleObjectNode.scaleOffset` 默认值是 `Vector3.one`，但代码是**加算** `localScale += 值`（等于"不改参数、每执行一次就放大 1"）—— 笔误已修，现在默认 `Vector3.zero` |
| 刚体 | `RigidbodyOpNode` / `Rigidbody2DOpNode` | **各一个节点覆盖 3 个 / 2 个**（模式枚举：施加力（ForceMode）/ 设置速度 / 设置角速度 —— 2D 无角速度）。原 5 个节点已合并删除。⚠️ 3D 与 2D **必须分开**：端口类型 `Vector3`/`Rigidbody` 与 `Vector2`/`Rigidbody2D` 不同，而**端口类型不能靠枚举切换** |
| 生成/销毁 | `SpawnObjectNode` / `DestroyObjectNode` | 实例化预制体（位置/旋转可接线、可选父物体、输出生成物体）、销毁物体（可延迟） |
| 物理 | `PhysicsRaycastNode` / `PhysicsRaycast2DNode` / `PhysicsOverlapSphereNode` / `PhysicsOverlapCircleNode` | 3D/2D 射线检测、球形/圆形范围检测；输出是否命中、命中点/法线/距离、命中物体、命中数量（索引取物体，帧缓存同帧共享） |
| 音频/动画 | `PlayAudioSourceNode` / `StopAudioNode` / `PlayAnimationNode` | **播放目标物体自带的 AudioSource**（模式 `Play`：剪辑可留空 = 用音源自带的；`PlayOneShot`：必须指定剪辑、可叠加）；停止该音源；播放动画状态（模式枚举 `Play` / `CrossFade` + 过渡时长）。⚠️ 原 `PlayAudioNode`（音频/播放）**已删除** —— 它继承 `ComponentActionNode<AudioSource>` 却完全不用那个目标、实际调 `MusicManager.PlaySFX`，是「音频/播放音效」`PlaySFXNode` 的劣质重复 |
| 音频（全局服务） | `PlaySFXNode` / `PlayMusicNode` / `StopMusicNode` / `SetMusicVolumeNode` / `SetSFXVolumeNode` | 走 `MusicManager` **单例**：音效（PlayOneShot 可叠加）/ 音乐（单轨可循环）/ 停音乐 / 音乐·音效音量（`FlowNode`，**不需要目标物体**） |
| 动画 | `SetAnimatorParameterNode` / `PlayAnimationNode` | **设置 Animator 参数**（`TypedValue` 值联合：`None`=Trigger / Bool / Float / Int —— 原「触发/布尔/浮点/整数参数」四个节点已合并删除）；**播放状态**（模式枚举 `Play` / `CrossFade` —— 原「交叉淡入」节点已合并删除） |
| 插值 | `MoveToNode` / `FadeCanvasGroupNode` / `TweenMaterialFloatNode` | 位置插值移动、CanvasGroup 透明度渐隐渐显、**材质浮点属性插值**（走 `MaterialPropertyBlock`）。三者都继承 `TweenNodeBase` → **自带可在节点面板直接编辑的曲线**（默认线性 = 行为不变；曲线值 >1/<0 可做过冲/回弹）；结束精确归位 |
| 相机 | `SetVcamPriorityNode` / `SetVcamTargetNode` / `CinemachineImpulseNode` / `SetVcamNoiseNode` / `SetDollySpeedNode` / `TargetGroupAddMemberNode` | Cinemachine：优先级切换相机、**跟随·注视目标**（模式枚举，原「设置跟随」「设置注视」两个同构节点已合并删除）、震屏、噪声振幅、轨道小车速度、目标组添加成员（依赖 Cinemachine 2.x 包） |
| 对话UI | 设置：`SetDialogueTextNode` / `SetDialogueSpriteNode` / `SetDialogueAudioClipNode`；获取：`GetDialogueTextNode` / `GetDialogueSpriteNode` | 设置对话文本（`startTypewriter` 默认开；**`waitUntilTyped` 默认开** → 本节点内等打字结束，一个节点搞定但无超时保护；取消它并把输出端口 `typingFinished` 接到「流程/等待条件」→ 改由等待节点等，**白拿 `timeout` 与 `timeoutTo` 超时分支**。两者互斥）、**三张图片之一**（设置侧与读取侧共用同一个 `DialogueImageSlot` 槽位枚举：边框 / 背景 / 人物 —— 原 3+3 个逐行同构节点已合并删除，设置侧的读回校验保留）、打字机对话音效；读取对话内容与三张图片 |
| 材质 | `SetMaterialPropertyNode` / `GetMaterialFloatNode` / `GetMaterialColorNode` / `TweenMaterialFloatNode` | 通过 **`MaterialPropertyBlock`** 读写材质属性（不改材质资产、不实例化材质副本、每个物体独立）；设置节点用 `TypedValue`，**一个节点覆盖 浮点/整数/颜色/向量/贴图**；读取节点读 `sharedMaterial`（材质资产上的值，不含 MPB 覆盖）；**浮点插值**节点带曲线，起始值默认读当前有效值（先看 PropertyBlock 覆盖再读材质），也可用 `overrideStart` 显式指定 |
| 选项UI | `ShowOptionsNode` | **一个节点顶原来的三个**（显示 + 等待 + 取索引合一）：显示一组选项（`List<string>` + 可选 `List<Sprite>` 图标）→ 默认在本节点内等待选择；继承 `ConditionWaitNode` 所以**白拿 `timeout` / `timeoutTo` 超时分支**；输出端口「选择完毕」(`bool`) 与「选中索引」(`int`，接「流程分支/按索引分支」)。关掉 `waitForSelection` 就变成"只显示"，改用「流程/等待条件」来等（与对话文本节点的两种用法完全对称） |
| 流程分支 | `BranchNode` / `MultiBranchNode` / `IndexBranchNode` | 双分支 / 多分支（`bool[]`）/ **按索引分支**（`int` → 动态分支端口，未接或越界沿 `next`） |
| 逻辑/常量/取值 | `AndLogicNode` / `OrLogicNode` / `NoLogicNode` / `BoolValueNode` 等 | 逻辑门、常量、类型转换 |
| 数学运算 | `MathOpIntNode` / `MathOpFloatNode` / `CompareIntNode` / `CompareFloatNode` / `RandomIntNode` / `RandomFloatNode` | 四则运算、比较、随机整数/浮点 |
| 字符串 | `StringConcatNode` / `StringOpNode` / `StringCompareNode` / `StringLengthNode` / `StringSubstringNode` / `StringReplaceNode` | 多段拼接（可选分隔符、可跳空段）、大小写/去首尾空格、比较（等于/包含/开头/结尾）、长度、截取、替换 |
| 转换 | `IntToFloatNode` / `FloatToIntNode` / `IntToStringNode` / `FloatToStringNode` / `StringToIntNode` / `StringToFloatNode` | int↔float↔string 互转 |
| 流程 | `PrintNode` / `WaitNode` / `WaitUntilNode` | 日志输出 / 等待指定秒数 / 等待条件成立（可接比较·逻辑·变量节点；支持**超时**，且**超时可走单独分支** `timeoutTo`，不接则超时也沿 `next`） |
| 流程控制 | `ForLoopNode` / `WhileLoopNode` / `ForEachLoopNode`（5 类型） / `ParallelNode` / `JumpToEntryNode` / `TimerNode` | 计数循环 / 条件循环 / 遍历列表 / 并行分支（最多 4 条）/ 跳转到入口（执行后当前链结束）/ 计时器（间隔 tick，0=无限） |

## TypedValue（类型枚举 + 值联合）

一个可序列化的「类型下拉 + 值字段」组合，用来**把"每种类型一个节点"合并成一个节点**：

```csharp
public TypedValue value = new TypedValue();   // 节点上的一个普通字段（非端口）
```

- **类型**：`None`（无值，如 Animator 的 Trigger）/ `Bool` / `Int` / `Float` / `String` / `Vector2` / `Vector3` / `Color` / `GameObject` / `Sprite` / `AudioClip` / `Texture`；
- 面板由 `TypedValueDrawer` 画成两行（类型下拉 + 按类型显示的值字段，其余隐藏），`GetPropertyHeight` 返回两行高度；
- **为什么在节点体里也能正常绘制**：xNode 的 `NodeEditorGUILayout.PropertyField` 对**非端口**属性直接调用 `EditorGUILayout.PropertyField`（见其源码第 40 行），因此 `[CustomPropertyDrawer]` 与多行高度都会生效 —— 这是这套机制能用在节点上的前提；
- 值字段全部 `[HideInInspector]`：默认遍历不会重复画它们；绘制器用 `FindPropertyRelative` 单独取出当前类型那一个（HideInInspector 只影响可见性遍历，不影响查找）；
- ⚠️ `GameObject` 类型同样受 Unity 限制**只能放资产 / 预制体**（场景物体拖不进节点字段）；`Sprite` / `AudioClip` / `Texture` 是资产，不受影响。

**收益**：材质属性设置因此是**一个节点**（浮点/整数/颜色/向量/贴图）而不是五个。
后续可同样合并：Animator 参数（`SetAnimatorFloat/Bool/Int/Trigger` 四个 → 一个）。

## 插值节点（TweenNodeBase）

**所有插值节点都继承 `TweenNodeBase`，从而自动获得"可在节点面板直接编辑的曲线插值"**：

```csharp
public abstract class TweenNodeBase : ComponentActionNodeBase
{
    public float duration = 1f;                                        // 可接线的输入端口
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);   // 节点面板上直接编辑

    protected abstract bool ResolveTarget();       // Execute 时解析目标（失败请自行警告 + 返回 false）
    protected abstract bool CaptureStart();        // 记录起始值（返回 false = 放弃本次插值）
    protected abstract void ApplyStep(float mix);  // 按混合系数写一帧
    protected static float / Vector3 Mix(start, end, mix);   // mix >= 1 时精确返回终点
}
```

**曲线作用在混合系数上**：先算线性进度 `k`(0~1)，再取 `curve.Evaluate(k)` 作为最终系数。

| 要点 | 说明 |
|---|---|
| **默认线性** | 默认曲线是 `AnimationCurve.Linear` → 行为与"没有曲线"**完全一致**，旧图升级后不变；`curve` 为 null / 空时也会自动退回线性（防御序列化意外） |
| **过冲 / 回弹** | 曲线值 >1 或 <0 会超出目标再回来，**这是有意支持的**、不做统一裁剪；需要限制范围的子类自己在 `ApplyStep` 里 Clamp（如透明度的 `alpha`） |
| **结束精确归位** | `Mix(start, end, mix)` 在 `mix >= 1` 时**直接返回终点**，避免浮点残差；若曲线末端 <1 则停在曲线末端值 |
| **"等待结束"是内在的** | `GetFlow()` 跑完整个插值才继续下一个节点 —— 不需要"是否等待"开关，也不需要"是否完成"输出端口；要并行就接「流程/并行」 |
| **节点会自动变高，不会裁剪** | xNode 把节点体画在高 **4000** 的区域内，并在每次 `Repaint` 用 `GUILayoutUtility.GetLastRect().size` 回写节点尺寸（见 `NodeEditorGUI.NodeGUI`），所以曲线编辑器能完整显示 |

**新增插值节点 = 继承 `TweenNodeBase` + 实现那三个方法。** 现有示例：`MoveToNode`（位置）、`FadeCanvasGroupNode`（透明度）、`TweenMaterialFloatNode`（材质浮点属性 + `MaterialPropertyBlock`）。

## 执行流程（GraphExecutor）

> 执行器枚举、触发策略、多链并发与事件订阅的**详细说明**见 [GraphExecutor.README.md](GraphExecutor.README.md)。

1. `Awake`：绑定图到挂载物体、注册到 `GraphCommunicator`；
2. `Start`：`autoExecute` 启动默认链；`entryEventSubscribe != Off` 时订阅入口事件；
3. **多链并发**：每条链是独立协程（每次事件/触发启动一条），互不打断；每链每 `executeInterval` 秒从起点沿 `GetConnectedNode()` 走链式执行（上限 100 次防死循环）；
4. `executeCount`（0 = 无限循环）按链独立计数；`[ContextMenu("执行节点图")]` 可手动触发；
5. 触发策略 `triggerPolicy` **按同一起点生效**：`Restart`（默认，重触发=停止该起点旧链并重跑）/ `IgnoreWhileRunning`（运行中忽略）/ `Queue`（运行中排队，当前链跑完自动再跑一轮）；
6. 执行游标为**执行器私有**，多个执行器跑同一张图互不干扰；共享图变量（并发链合作/独立按变量划分）；

## 数据/类型扩展：Vector2 与列表变量

在 string/bool/int/float/Vector3 之外新增两种数据能力，**全栈打通**（变量容器 → 存档 → 跨图/房间/全局 → 子图参数 → 节点）：

- **Vector2**：变量 Get/Set（`变量操作/获取|设置/Vector2`）、子图参数（`子图/参数输入|输出/二维向量`）、常量（`值/二维向量`）、转换（`取值/转换/二维向量(两个浮点)`、`三维向量(二维向量)`、`二维向量(三维向量)`）、数学（`数学运算/二维向量运算`、`二维向量缩放`）。
- **列表变量（List）**：五种元素类型（字符串/整数/浮点/Vector2/Vector3），在图资产或 VariableBundleObject 的 Inspector 里定义初始列表；
  - Get/Set 节点：`变量操作/获取|设置/字符串列表` 等（source 同普通变量）；
  - 子图参数：`子图/参数输入|输出/字符串列表` 等；
  - 操作节点（`列表/…`）：`添加`（追加元素）、`移除`（按值移除）、`取元素`（索引取值，越界警告）、`数量`、`是否包含`；
  - **引用语义**：Get 列表节点返回的是变量容器里的列表**引用**，`列表/添加`、`列表/移除` 直接改引用即写回变量（无需再 Set）；列表为 null（未定义）时操作节点警告并跳过；
  - 存档：Vector2 与列表都进 `VariableBundleData`（旧存档缺字段 → 导入时安全跳过，不破坏旧档）。
- 新增类型全部可作子图参数传递、可存房间/全局持久变量（`PersistentVariableManager` 已订阅对应事件通道）。

## 外部传参（外部代码 → 节点图）

外部 C# 代码（如 Unity Input 系统处理器）可在触发图入口时携带**命名参数包**，图内用「参数/输入/xxx」节点读取：

```csharp
GraphParams p = new GraphParams();
p.Set("move", new Vector2(0, 1f));   // 输入轴
p.Set("jump", true);                  // 按键
executor.ExecuteFromEntry("OnInput", p);   // 直调
// 或事件路径：
GraphEvent.Trigger(e => { e.eventId = "OnInput"; e.data = p; });
```

- **图内读取**：`参数/输入/字符串|布尔|整数|浮点|二维向量|三维向量|物体|…列表` 节点（即统一参数输入节点，原子图参数节点家族），`paramName` 与传入键一致，未命中/类型不符返回节点字段默认值；
- **瞬态语义**：参数存于图资产的**统一调用参数存储**（非序列化，不进存档、不进 VariableBundle、不进编辑器下拉）；每次**带参**触发先清空上一批再注入（替换语义）；`GraphExecutor.ClearInvocationParams()` 可主动清空；
- **触发 API**：`ExecuteFromEntry(entryId, args)`（标识符/GUID）；`ExecuteFrom(start, args)`；`GraphEvent.data` 载荷；`GraphEventEmitter` 仍是无参触发；
- **面板可视化编辑**：任何外部 MonoBehaviour 声明 `public GraphParamList xxx;` 即可在 Inspector 里增删/改参数（名称 + 类型下拉 + 按类型显示的值字段，`GraphParamEntryDrawer` 绘制）；运行时 `xxx.Build()` 出 `GraphParams`。开箱即用的 `GraphParamEmitter`（挂场景物体）：Inspector 编辑参数包 + eventId，按钮/UnityEvent 拖 `Emit()` 即**带参**触发事件；
- **初始参数**：把 `GraphParamEmitter` 放在执行器同一物体（或其子物体）上，其参数会在 `GraphExecutor.Awake` 自动注入为图调用参数的**初始值**——图启动时「参数/输入」节点即可读到配置值；之后带参触发（`ExecuteFromEntry`/事件/状态机）按替换语义覆盖，不带参触发保留初始值；
- **统一**：图内没有独立的"子图参数"与"外部参数"——`参数/输入`、`参数/输出` 是**唯一**的参数节点（`SubGraphInputNode`/`SubGraphOutputNode` 家族），所有调用方（子图节点 / 外部代码 / 事件 / 状态机）都注入同一份图调用参数存储：**同一张图既被子图节点调、也被外部直接调，参数节点完全通用（父图随时可变子图）**；
- **返回值外部读回**：外部代码执行后 `graph.GetOutputValue<T>(paramName)`（或 `executor.GetOutput<T>(paramName)`）读取图内「参数/输出」节点求值；
- **状态机带参**：`GraphStateMachine.TransitionTo(stateName, GraphParams)`；

## 组件动作节点（ComponentActionNode）

统一"目标解析 + 组件获取"的泛型基类 `ComponentActionNode<T>`：

- **目标只有一个来源**：基类的 **GameObject 输入端口**（`targetGameObject`，非序列化，不显示值框）。请把「取值/获取物体(自身|名称|引用)」接上去显式指定目标；**未接线 → 警告并跳过该节点**；
- 自动 `GetComponent<T>`，找不到给出警告；
- 子类只需实现 `Apply(T component)` 做具体动作；
- 加新操作（缩放 / 音频 / 动画 / 对话音效等）= 继承基类 + 一个 `Apply`；
- 无内置的"图绑定物体 / 按名字 / 拖引用"回退模式——已拆成下面一整个获取物体节点族。

## 全局服务节点（ServiceNodes）

作用于**全局单例 / 静态服务**的动作节点（例：`MusicManager` 播音乐、播音效、调音量）：

- **不继承** `ComponentActionNodeBase`，**直接继承 `FlowNode`** —— 它们没有"目标物体"这个概念，硬继承只会凭空多出一个必须接线、否则警告跳过的 `targetGameObject` 端口；
- 统一放在 `Nodes/ServiceNodes/`，与 `ComponentNodes/` 并列；
- **菜单仍按功能取前缀**（如 `音频/…`）—— `CreateNodeMenu` 是给用户按功能找的，**文件夹分类 ≠ 菜单分类**（项目本来如此：`ComponentNodes/AudioNodes/` 的菜单就是 `音频/`）；
- **没有基类**：各服务节点调用的单例/API 各不相同，除了 `NodeLog` 几乎没有共同代码，抽个空基类只会多一层间接。

> 判据：**需要"指定哪个物体"的 → `ComponentNodes/` + `ComponentActionNode<T>`；作用于全局单例 / 静态服务的 → `ServiceNodes/` + `FlowNode`。**

注意音频这两个**不是重复能力**：
- `音频/播放`（`PlayAudioNode`）＝ 播放**目标物体自带**的 `AudioSource`（适合 3D 空间音、预先配好 clip/volume/loop 的音源）；
- `音频/播放音效`（`PlaySFXNode`）＝ 走 `MusicManager` 的**共享** SFX 音源（`PlayOneShot`，可叠加，不关心位置）。

## 物体引用（获取物体节点族）

目标来源已拆成一组独立的数据节点（共 7 个，`GetObjectNodeBase : DataNode` 的子类），输出 `GameObject` 端口，接到任意 `ComponentActionNode` 的「目标物体」输入端口（或经子图 GameObject 参数传入子图）：

| 菜单 | 类 | 语义 |
|---|---|---|
| **取值/获取物体(自身)** | `GetSelfObjectNode` | 输出图绑定物体＝**当前执行器所在物体**（`NodeExecuteContext.Current`；非执行期回退图资产 `attachedObject`） |
| **取值/获取物体(名称)** | `GetNamedObjectNode` | 在图绑定物体**自身及直接子物体**里按名查找；`objectName` 为 `string` 输入端口（可接线，未接线用字段值） |
| **取值/获取物体(全场景)** | `GetSceneObjectNode` | 在**所有已加载场景**（含 DontDestroyOnLoad）里按名查找，含未激活；`objectName` 为 `string` 输入端口（可接线，未接线用字段值）。**带缓存**（键=名称，命中后不再扫描；物体被销毁靠 Unity 的 fake-null 自动重扫） |
| **取值/获取物体(父物体)** | `GetParentObjectNode` | 输出「子物体」端口的 `Transform.parent`；子物体为空或它是根物体时警告并返回 null |
| **取值/获取物体(子物体)** | `GetChildObjectNode` | 取**任意物体**的子物体：`mode` = `ByIndex`（索引，越界警告）/ `ByName`（名称，`recursive` 可递归整棵子树）；含未激活 |
| **取值/获取物体(根物体)** | `GetRootObjectNode` | 输出「子物体」端口所在层级的**最顶层物体**（`Transform.root`）——等于一次调用替代串联多个「父物体」 |

命名对应关系：**往上走**的输入端口叫「子物体」（父物体 / 根物体），**往下走**的叫「父物体」（子物体）。

- 共用基类 `GetObjectNodeBase`（统一 `[Output] GameObject output` 非序列化端口 + 运行时求值 + `AttachedObject` 取值）；**非序列化端口不需要注册编辑器** —— 默认节点体编辑器 `BaseNodeBodyEditor`（注册在 `BaseNode` 上）已继承 `VisiblePortsNodeEditor`，所有节点都会自动补画；
- 输出口非序列化（运行时求值，规避场景引用写进图资产 / 跨场景重载失效）；
- ⚠️ 「名称」只查一层子物体（`transform.Find`），不递归；「名称等于图绑定物体自身」也算命中；
- ⚠️ **把「场景物体」送进图的三条正确途径**（**不存在**"把场景物体拖进节点字段"这一条——节点存在图资产里，Unity 禁止资产持有场景引用）：
  1. **参数注入（最可靠）**：「参数/输入/物体」节点 + 场景侧 `GraphParamEmitter`（加一条 `GameObject` 类型参数指向目标）。引用由**场景上的 MonoBehaviour** 持有，经统一调用参数存储注入，**不写进图资产** → 改名 / 重名 / 跨场景都不会错；
  2. **用结构推导**：「自身 / 子物体 / 父物体 / 根物体」把目标算出来（不存引用）——适合"目标总在执行器层级里"这类关系；
  3. **按名字**：「名称 / 全场景」——适合名字稳定的常驻物体；改名或重名有风险（全场景节点已加同名警告）。

> 原「取值/获取物体」（`GetGameObjectNode`，`source` = `Self`/`All`）与其配套缓存类 `SceneObjectFinder.cs` 均已删除，能力由上面这组节点覆盖。
> 「全场景」节点**带缓存**：键 = 对象名称，命中后不再扫描 —— 接到"每帧驱动"的组件节点或逐帧轮询的「流程/等待条件」上也没问题。
> 缓存写在**节点实例字段**上（不是静态字典）：键变即作废；物体被销毁时 Unity 的 fake-null 会让它自动重扫，**不需要任何失效订阅**。
> ⚠️ 只有"**结果与执行宿主无关**"的节点才能这样缓存。「名称」节点的结果依赖隐藏的"图绑定物体"，缓存会把不同执行器的物体串味，所以**故意不缓存**。

## 图状态机（GraphStateMachine）

把"状态 = 一张子图"的状态机挂在场景物体上（复用 GraphChainRunner + 子图机制，不依赖 GraphExecutor）：

- **状态列表** `List<GraphState>`：`stateName` + 状态子图 + `entryIdentifier`（入口标识，空 = 子图默认起点）+ `loop`（链跑完是否循环重跑）；
- **切换**：`TransitionTo(stateName)`（C# / UnityEvent 调用，或图内用菜单 **状态机/切换** 节点）；`TransitionTo(stateName, GraphParams)` 可携带调用参数注入状态子图；
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
- Inspector 里入口模式下提供下拉选择器；标识符改名后会自动回填修正（编辑模式）；
- **入口独立触发策略**：每个入口可勾选「触发策略覆盖」配置自己的 `Restart` / `IgnoreWhileRunning` / `Queue`，否则跟随执行器的 `triggerPolicy`（各入口是独立起点，链状态互不影响）。

## 节点图 GUID（存档键）

- 每张图有稳定 `GUID`（`BaseNodeGraph.Guid`），**存档以图 GUID 为键**；
- 新图首次保存后持久化；`[ContextMenu("重新生成GUID")]` 可手动更换（**会破坏存档对应关系**）。

## 事件支撑

通讯 / 持久变量节点通过 `ParameterizedEvent` 泛型事件与 `GraphCommunicator`、`PersistentVariableManager` 交互（事件类集中在 `CommunicationNodes/`：`ComSetAndGetVariableEvent`、`PersistentVariableEvent`）；`GraphCommunicator` 启动时自动读档。

- **触发器入口标识下拉**：`GraphEventEmitter` / `CollisionEventEmitter` 的 Inspector 会扫描场景中所有执行器使用的节点图及其入口，提供 eventId 下拉（避免手填拼错）；选中后回填入口标识符，未命中显示警告，仍可手动输入。复用：自定义触发器编辑器调用 `GraphEventEntryOptionPicker.CollectEntryOptions()` + `DrawEventIdPicker()`。

## 编辑器工具

- **节点浏览器**：`Tools/节点系统/节点浏览器`，或节点图空白处右键 →「打开节点浏览器…」。支持**搜索**（按菜单路径/类名）、**最近使用**（自动记录最近创建的 12 种节点）、**收藏**（★ 标记，EditorPrefs 持久化）。节点图空白处右键菜单顶部也会显示「★ 收藏」「最近使用」快捷项与浏览器入口；
- **变量引用下拉**：Get/Set 变量节点的节点体里，「变量名」提供**图中已定义变量**下拉（`BaseNodeGraph.GetAllVariableNames()` 扫描 VariableBundle 全类型去重），未命中保留手动输入；
- **拖拽场景物体建节点**：把 Hierarchy 里的物体拖进图编辑器 → 在落点生成「**取值/获取物体(全场景)**」节点，`objectName` = 物体名（多选拖拽逐个生成、竖向错开）。只认场景物体，拖入预制体/脚本等资源不处理；**当前场景存在同名物体时会打警告**并给出拖入物体的层级路径——因为「全场景」按层级顺序取第一个，同名时可能不是你拖的那个，这时请改用「取值/获取物体(引用)」；
- 已有：入口下拉、触发器 eventId 下拉、日志级别菜单、多链运行高亮、变量 GUID 自动回填。

## 子图封装（SubGraphNode）

把一段逻辑封装成另一张节点图，在父图里当一个节点调用（菜单 **子图/执行**）：

- **子图准备**：子图 = 普通 `BaseNodeGraph`；用「参数/输入」「参数/输出」节点声明出入口参数（**参数名 = 调用参数键**，图中唯一）；入口用 StartNode 或 EntryNode，EndNode 收尾；
- **参数端口**：`SubGraphNode` 选好子图后**自动生成与参数节点一一对应的输入/输出端口**（端口名 = 参数名），连线即传参。执行时：父图连线值 → **子图统一调用参数存储** → 跑子图链 → 输出节点输入求值 → 输出端口；
- **GameObject 参数**：「参数/输入/物体」「参数/输出/物体」与各基础类型/列表参数并列；**不走变量系统**（GameObject 不入 VariableBundle 序列化）——统一走调用参数存储注入/求值回读，端口同步/嵌套/循环校验逻辑完全复用；
- **子图内部接法**：**参数输入节点 = 取值源**（输出端口，连到需要参数的地方，未注入时用节点字段默认值）；**参数输出节点 = 返回值槽**（输入端口，把结果连进来，链跑完后父图读回）；
- **执行语义**：同步阻塞（父链等子图跑完，与 Wait 节点一致）；目标解析沿用父执行器（Attached 目标 = 父执行器物体）；嵌套深度上限 8（运行时拦截），编辑器做循环引用与参数重名校验；
- **注意**：同一子图被多条链并发调用时**调用参数/变量共享**（要隔离就复制子图资产）；`resetVariablesOnCall` 重置子图变量（调用参数每次调用重新注入，不受影响）；参数改名会断开对应端口连线。

## 扩展模式

新节点 = 继承基类 + `[CreateNodeMenu]` 特性：

```csharp
[CreateNodeMenu("变量操作/获取/浮点")]
public class GetVariableFloatNode : GetVariableNode<float> { }
```

五条约定：

1. **非序列化端口不用管**：`[Input]`/`[Output]` + `[System.NonSerialized]` 的字段会被默认节点体编辑器自动补画（`BaseNodeBodyEditor` 注册在 `BaseNode` 上、已继承 `VisiblePortsNodeEditor`），**不需要再写 `[CustomNodeEditor]` 注册**。只有要**定制画法**（改绘制 / 加下拉 / 隐藏字段）时才注册——例如 `SubGraphInputNodeBodyEditor`。
2. **"每种类型一个节点"先考虑 `TypedValue`**：如果几个节点只差值类型（Animator 参数、材质属性都属此类），就用**一个节点 + `TypedValue` 字段**，而不是写 N 个同构类。
3. **需要目标物体的动作节点**继承 `ComponentActionNode<T>`（自带「目标物体」输入端口 + 组件获取 + 警告）。复杂一点的（如材质读写）可自己声明 `targetGameObject` 端口 + 用 `NodeTargetResolver.Resolve<T>(this)` 收拢解析逻辑。
4. **不要在节点里放序列化的 `GameObject` 字段指向场景物体** —— Unity 不允许图资产持有场景引用，那个字段**根本拖不进场景物体**（对象选择器只列资产）。要把场景物体送进图，走「参数/输入/物体」+ 场景侧 `GraphParamEmitter`（见上文「把场景物体送进图的三条正确途径」）。
5. **插值节点继承 `TweenNodeBase`**（不要直接继承 `ComponentActionNode`）—— 自动获得可编辑曲线、统一的 `duration` 端口与"结束精确归位"，见上文「插值节点（TweenNodeBase）」。

