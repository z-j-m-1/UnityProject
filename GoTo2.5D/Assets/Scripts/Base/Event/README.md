# 事件系统

轻量级泛型事件总线，带对象池回收。命名空间 `ZGameFramework.Core`。

## 核心类

| 类 | 说明 |
|---|---|
| `EventBus` | 静态事件总线（发布 / 订阅 / 退订） |
| `GameEvent` | 事件基类（实现 `IPoolable`） |
| `ParameterizedEvent<T>` | 泛型参数事件：`Trigger` / `TriggerAsync` / `TriggerAsyncRecycle` / `Subscribe` / `Unsubscribe` |
| `SignalEvent<T>` | 无参信号事件 |
| `ClassPool<T>` / `ListPool<T>` | 对象池 |

## 用法

```csharp
// 定义事件
public class MyEvent : ParameterizedEvent<MyEvent>
{
    public int value;
    public override void OnRecycled() { value = 0; }
}

// 订阅
MyEvent.Subscribe(e => Debug.Log(e.value));

// 触发（同步发布，订阅者立刻收到，回调返回后自动回收）
MyEvent.Trigger(e => e.value = 42);
```

## 说明

- `Trigger` 同步发布事件，回调内可安全读取数据；事件对象从池复用，回调返回后自动回收；
- 异步高频事件用 `TriggerAsyncRecycle`（不自动回收），需外部手动 `ClassPool<T>.Recycle`；
- 事件字段记得在 `OnRecycled` 清理，防止复用脏数据。
