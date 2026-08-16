using System.Collections.Generic;
using UnityEngine;
using XNode;

/// <summary>获取列表变量-字符串（source 枚举选择本图/跨图/房间/全局）</summary>
[CreateNodeMenu("变量操作/获取/字符串列表")]
public class GetVariableStringListNode : GetVariableNode<List<string>>
{
}