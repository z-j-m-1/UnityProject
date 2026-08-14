# UI 系统

## TextEvents

挂在 UI 文本（TextMeshProUGUI）上，提供：

| 方法 | 说明 |
|---|---|
| `UpdateText(string newText)` | 直接设置文本 |
| `UpdateTextWithRichText(string newText)` | 用 `<wave>` 富文本包裹设置 |

## UI 通讯节点（`xNode/Nodes/UICommunicatorNodes`）

跨图通讯更新 UI：

- `ComGetSelfTextNode` / `ComSetSelfTextNode`
- `ComGetSelfTextMeshProNode` / `ComSetSelfTextMeshProNode`

由 `UICommunicator`（单例）按"图 + UI 名"缓存组件引用，节点图里可直接读写 UI 文本。
