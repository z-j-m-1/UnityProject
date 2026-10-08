using UnityEngine;

/// <summary>
/// 子图参数输出-曲线（AnimationCurve）：父图 SubGraphNode 会自动生成同名输出端口。
/// 子图内部把曲线连到本节点的「值」输入端口，子图链跑完后父图读回。
/// </summary>
[CreateNodeMenu("参数/输出/曲线")]
public class SubGraphOutputCurveNode : SubGraphOutputNode<AnimationCurve>
{
}
