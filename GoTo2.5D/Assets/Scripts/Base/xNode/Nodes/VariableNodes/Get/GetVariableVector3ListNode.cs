using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>获取列表变量-三维向量（source 枚举选择本图/跨图/房间/全局）</summary>
[CreateNodeMenu("变量操作/获取/Vector3列表")]
public class GetVariableVector3ListNode : GetVariableNode<List<Vector3>>
{
}