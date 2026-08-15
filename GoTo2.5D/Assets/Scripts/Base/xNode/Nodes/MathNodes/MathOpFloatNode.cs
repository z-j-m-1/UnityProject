using UnityEngine;
using XNode;

/// <summary>数学运算-浮点（加/减/乘/除）</summary>
[CreateNodeMenu("数学运算/浮点运算")]
public class MathOpFloatNode : MathOpNode<float>
{
    protected override float Calculate(float a, float b)
    {
        switch (operation)
        {
            case MathOperation.Add: return a + b;
            case MathOperation.Subtract: return a - b;
            case MathOperation.Multiply: return a * b;
            case MathOperation.Divide:
                if (Mathf.Approximately(b, 0f))
                {
                    Debug.LogWarning($"{GetType().Name}: 除数不能为 0，返回 0");
                    return 0f;
                }
                return a / b;
            default:
                return a;
        }
    }
}
