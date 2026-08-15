using UnityEngine;
using XNode;

/// <summary>数学运算-整数（加/减/乘/除）</summary>
[CreateNodeMenu("数学运算/整数运算")]
public class MathOpIntNode : MathOpNode<int>
{
    protected override int Calculate(int a, int b)
    {
        switch (operation)
        {
            case MathOperation.Add: return a + b;
            case MathOperation.Subtract: return a - b;
            case MathOperation.Multiply: return a * b;
            case MathOperation.Divide:
                if (b == 0)
                {
                    Debug.LogWarning($"{GetType().Name}: 除数不能为 0，返回 0");
                    return 0;
                }
                return a / b;
            default:
                return a;
        }
    }
}
