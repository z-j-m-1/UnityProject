using UnityEngine;

/// <summary>
/// 子图参数输入-曲线（AnimationCurve）：
/// 父图的 SubGraphNode 会**自动**生成同名的 AnimationCurve 输入端口（端口类型取自本节点的 ParamType），
/// 所以 SubGraphNode 本身不需要任何改动。子图内部连本节点的「值」输出端口取这条曲线。
/// </summary>
[CreateNodeMenu("参数/输入/曲线")]
public class SubGraphInputCurveNode : SubGraphInputNode<AnimationCurve>
{
}
