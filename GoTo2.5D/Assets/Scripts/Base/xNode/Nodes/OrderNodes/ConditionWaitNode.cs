using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 条件等待节点基类 - 等待某个条件成立后继续执行链。
/// 扩展方式：
///   1. 纯数据条件（变量比较 / 逻辑运算等）→ 直接用 WaitUntilNode，把条件接入 condition 输入端口
///   2. 系统级条件（对话选项 / 打字结束 / 动画完成等）→ 继承本类，实现 CheckCondition（可选覆写 OnWaitStart 做准备工作）
///
/// 出口有两个：
///   next      = 条件成立后继续（FlowNode 自带）
///   timeoutTo = 超时分支；**未接线时超时也沿 next 继续**，与加分支前的旧行为完全一致
///
/// ⚠️ TimedOut / 等待结果存在节点实例上：同一张图被多条链并发跑过本节点时会互相覆盖
///    （与 ForLoopNode.index、MoveToNode.targetTransform 等共享节点状态的性质相同）。
/// </summary>
public abstract class ConditionWaitNode : FlowNode
{
    [Header("等待条件")]
    [Tooltip("超时秒数，0 = 不限时")]
    public float timeout = 0f;

    [Header("超时后走这里（未接线 = 超时也沿 next 继续）")]
    [Output(ShowBackingValue.Never, ConnectionType.Override)]
    public BaseNode timeoutTo;

    /// <summary>最近一次等待是否因超时结束（供 GetConnectedNode 选择出口）</summary>
    protected bool TimedOut { get; private set; }

    public override void Execute()
    {
        OnWaitStart();
    }

    public override IEnumerator GetFlow()
    {
        TimedOut = false;

        if (timeout <= 0f)
        {
            yield return new WaitUntil(CheckCondition);
            yield break;
        }

        float startTime = Time.time;
        bool conditionMet = false;
        yield return new WaitUntil(() =>
        {
            conditionMet = CheckCondition();
            return conditionMet || Time.time - startTime >= timeout;
        });

        // 用最后一次判定结果区分「条件成立」与「超时」，避免事后重算条件造成二次副作用
        TimedOut = !conditionMet;
        if (TimedOut)
        {
            bool hasBranch = GetTimeoutBranch() != null;
            NodeLog.Info($"{GetType().Name}: 等待超时（{timeout} 秒）{(hasBranch ? "，走超时分支" : "，未接超时分支，沿 next 继续")}");
        }
    }

    /// <summary>条件成立 → next；超时且接了超时分支 → timeoutTo；否则 → next</summary>
    public override BaseNode GetConnectedNode()
    {
        if (TimedOut)
        {
            BaseNode branch = GetTimeoutBranch();
            if (branch != null) return branch;
        }
        return base.GetConnectedNode();
    }

    /// <summary>开始等待时调用（可选覆写：显示选项、开始动画等）</summary>
    protected virtual void OnWaitStart() { }

    /// <summary>子类实现：条件成立返回 true</summary>
    protected abstract bool CheckCondition();

    /// <summary>取超时分支的连线目标（未接线返回 null）</summary>
    private BaseNode GetTimeoutBranch()
    {
        NodePort port = GetOutputPort(nameof(timeoutTo));
        if (port != null && port.IsConnected)
        {
            NodePort connection = port.GetConnection(0);
            if (connection != null) return connection.node as BaseNode;
        }
        return null;
    }
}
