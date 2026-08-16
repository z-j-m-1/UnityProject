using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>设置列表变量-三维向量（source 枚举选择本图/跨图/房间/全局）</summary>
[CreateNodeMenu("变量操作/设置/Vector3列表")]
public class SetVariableVector3ListNode : SetVariableNode<List<Vector3>>
{
}