# 变量系统

通用变量存储体系。节点图变量、房间变量、全局变量、物品状态共用同一套机制。

## 核心类

| 类 | 说明 |
|---|---|
| `Variable<T>` | 单个变量（Name / Value / Guid），序列化列表元素，GUID 自动生成 |
| `VariableContainer<T>` | 按类型（string / bool / int / float）的变量容器，含名字↔GUID 映射 |
| `VariableBundle` | 四类容器捆成一组，统一泛型读写 `Get<T>` / `Set<T>` / `Has<T>` / `TryResolve<T>` |
| `VariableBundleObject` | ScriptableObject 资产，Inspector 可直接编辑变量 |
| `PersistentVariableScope` | 枚举：`Room` / `Global` |
| `PersistentVariableManager` | 持久变量管理器基类（订阅事件 + Import/Export） |
| `RoomVariableManager` | 房间变量单例（优先用 RoomIdentity 资产，其次按场景名加载） |
| `GameGlobalVariableManager` | 全局变量单例（手动配置当前/默认变量对象） |
| `RoomIdentity` | 房间身份组件（roomId + 直接引用房间变量资产），抗场景改名 |

## 名字 ↔ GUID 解析（抗改名）

全模式 **名字优先 + GUID 兜底**：改名后旧引用靠 GUID 命中，GUID 变化后靠新名字命中。

- `VariableContainer<T>.TryResolve(name, guid, out value, out actualName, out actualGuid)` / `TryResolveAndSet(...)`
- `VariableBundle` / `VariableBundleObject` / `PersistentVariableManager` / `BaseNodeGraph` 均暴露同名泛型方法；
- Get/Set 节点面板显示 `variableGuid`（自动记录，命中后回填修正，双向适配）；
- 场景侧：`RoomIdentity` 直接引用房间变量资产（按资产 GUID 绑定），场景文件改名不影响加载。

## dict vs list 语义（关键）

- `variableList`：**设计默认值**，编辑模式唯一数据源（Inspector 直接编辑）；
- `runtimeDict`：**运行时当前值**，有存档则从存档构建，否则从列表构建；
- 运行时 `Set` 只写 dict；编辑模式（非运行）`Set` 才写列表。

## 持久化开关（persist）

每个变量有一个 `persist` 开关（Inspector 勾选，**默认开启**）：

- **关闭后**：不进存档（Export 跳过）、读档不覆盖（ImportFrom 跳过），**每次开始游戏都重置为列表初始值**；
- 适合临时状态：当前对话者、当次会话标记、临时计数等；
- 运行时新建的变量默认持久化（和现状一致）；
- 该开关对图 / 房间 / 全局变量通用（共用 `Variable<T>`）。

## 使用

```csharp
// 脚本直接访问
RoomVariableManager.Instance.Set("hp", 100f);
float hp = RoomVariableManager.Instance.Get<float>("hp", 0f);

GameGlobalVariableManager.Instance.Set("playerName", "小明");
string name = GameGlobalVariableManager.Instance.Get<string>("playerName", "");

// 名字优先 + GUID 兜底解析
if (BaseNodeGraph.TryGetVariable("hp", guid, out float v, out string actualName, out string actualGuid))
{
    // ...
}
```

## RoomIdentity 与编辑器工具

`RoomVariableManager` 加载顺序：**① RoomIdentity 直接引用的资产 → ② 按场景名 Resources 加载 → ③ 编辑器自动创建**。

编辑器提供菜单 **Tools/房间/RoomIdentity 补齐（当前场景 / Build Settings 所有场景）**：
自动创建 RoomIdentity 根物体（生成稳定 roomId）、创建/回填变量资产引用并保存场景（仅编辑模式可用，播放中菜单置灰）。

## 注意事项

- 房间变量资产命名 = 场景名（`PersistentVariables/Room/<场景名>.asset`），找不到时编辑器会自动创建；
- 推荐在房间场景挂 `RoomIdentity` 组件并拖入变量资产（或用编辑器工具一键补齐），之后场景改名不再影响加载；
- `itemId` / 图 GUID 生成后需在编辑器保存一次场景/资产才会持久化。
