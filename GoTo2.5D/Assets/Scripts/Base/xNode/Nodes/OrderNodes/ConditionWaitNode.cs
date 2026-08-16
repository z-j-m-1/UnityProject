using System.Collections;
using UnityEngine;
using XNode;

/// <summary>
/// 条件等待节点基类 - 等待某个条件成立后继续执行链
/// 扩展方式：
///   1. 纯数据条件（变量比较 / 逻辑运算等）→ 直接用 WaitUntilNode，把条件接入 condition 输入端口
///   2. 系统级条件（对话选项 / 动画完成等）→ 继承本类，实现 CheckCondition（可选覆写 OnWaitStart 做准备工作）
/// </summary>
public abstract class ConditionWaitNode : FlowNode
{
    [Header("等待条件")]
    [Tooltip("超时秒数，0 = 不限时")]
    public float timeout = 0f;

    public override void Execute()
    {
        OnWaitStart();
    }

    public override IEnumerator GetFlow()
    {
        float startTime = Time.time;
        if (timeout <= 0f)
        {
            yield return new WaitUntil(CheckCondition);
        }
        else
        {
            yield return new WaitUntil(() => CheckCondition() || Time.time - startTime >= timeout);
        }
    }

    /// <summary>开始等待时调用（可选覆写：显示选项、开始动画等）</summary>
    protected virtual void OnWaitStart() { }

    /// <summary>子类实现：条件成立返回 true</summary>
    protected abstract bool CheckCondition();
}
