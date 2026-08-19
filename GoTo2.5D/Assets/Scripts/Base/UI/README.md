# UI 系统

位置：`Assets/Scripts/Base/UI` + `Base/xNode/Nodes/UICommunicatorNodes`。

## UICollector（UI 收集者）

挂在 Canvas 上，收集其下所有 `Text` / `TextMeshPro` / `Image` 组件，按**物体名**索引。单例，**不 DontDestroyOnLoad**（随场景重建）。

- 自动找场景 Canvas；没有则创建（补 `CanvasScaler` + `GraphicRaycaster`）；
- 同类型同名会覆盖并打警告，请避免 UI 重名；
- `Find<T>(name)` 按类型 + 名字查找；`Refresh()` 重新收集。

## TextEvents

挂在 UI 文本（TextMeshProUGUI）上：

| 方法 | 说明 |
|---|---|
| `UpdateText(string)` | 直接设置文本 |
| `UpdateTextWithRichText(string)` | 用 `<wave>` 富文本包裹设置（TMPEffects 特效） |

## UI 通讯节点（`xNode/Nodes/UICommunicatorNodes`）

| 节点 | 菜单 |
|---|---|
| `ComUIGetTextNode` | `通讯UI/获取文本` |
| `ComUISetTextNode` | `通讯UI/设置文本` |

- `source`：`Self`（图附加物体的自身/子物体）或 `Canvas`（UICollector 任意 UI）；
- `uiType`：`Text` / `TextMeshPro`（`Image` 只收集、暂不支持读写）；
- Get 节点带 `stripRichText` 选项（剔除 `<...>` 富文本标签）。

## LongPressButton（长按按钮）

继承 `UnityEngine.UI.Button`（保留 `onClick` 与视觉状态），额外提供长按事件。菜单 `UI/Long Press Button`。

- `onLongPress`（UnityEvent）：按住超过 `longPressDuration`（默认 0.5s）触发，面板可拖（可直接接 `GraphEventEmitter.Emit()`）；
- `repeatInterval`：`0` = 只触发一次；`>0` = 按住期间按间隔重复触发；
- `suppressClickAfterLongPress`（默认开）：长按触发后本次松手不再触发 `onClick`（短按仍是普通点击）；
- 用 `Time.unscaledTime` 计时（暂停时也生效）；用法：把物体上的 `Button` 组件换成本脚本即可。
