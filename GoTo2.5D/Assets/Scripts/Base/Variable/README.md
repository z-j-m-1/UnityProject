# 变量系统

通用变量存储体系。节点图变量、房间变量、全局变量、物品状态共用同一套机制。

## 核心类

| 类 | 说明 |
|---|---|
| `Variable<T>` | 单个变量（Name / Value / Guid），序列化列表元素 |
| `VariableContainer<T>` | 按类型（string / bool / int / float）的变量容器 |
| `VariableBundle` | 四类容器捆成一组，统一泛型读写 `Get<T>` / `Set<T>` / `Has<T>` |
| `VariableBundleObject` | ScriptableObject 资产，Inspector 可直接编辑变量 |
| `PersistentVariableScope` | 枚举：`Room` / `Global` |
| `PersistentVariableManager` | 持久变量管理器基类（订阅事件 + Import/Export） |
| `RoomVariableManager` | 房间变量单例（按场景从 `Resources/PersistentVariables/Room/` 加载） |
| `GameGlobalVariableManager` | 全局变量单例（手动配置当前/默认变量对象） |

## dict vs list 语义（关键）

- `variableList`：**设计默认值**，编辑模式唯一数据源（Inspector 直接编辑）
- `runtimeDict`：**运行时当前值**，有存档则从存档构建，否则从列表构建
- 运行时 `Set` 只写 dict；编辑模式（非运行）`Set` 才写列表

## 使用

```csharp
// 脚本直接访问
RoomVariableManager.Instance.Set("hp", 100f);
float hp = RoomVariableManager.Instance.Get<float>("hp", 0f);

GameGlobalVariableManager.Instance.Set("playerName", "小明");
string name = GameGlobalVariableManager.Instance.Get<string>("playerName", "");
```

## 注意事项

- 房间变量资产命名 = 场景名（`PersistentVariables/Room/<场景名>.asset`），找不到时编辑器会自动创建
- `itemId` / 图 GUID 生成后需在编辑器保存一次场景/资产才会持久化
