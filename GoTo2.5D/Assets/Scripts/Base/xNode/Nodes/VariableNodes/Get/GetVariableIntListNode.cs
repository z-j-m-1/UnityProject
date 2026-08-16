using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>获取列表变量-整数（source 枚举选择本图/跨图/房间/全局）</summary>
[CreateNodeMenu("变量操作/获取/整数列表")]
public class GetVariableIntListNode : GetVariableNode<List<int>>
{
}