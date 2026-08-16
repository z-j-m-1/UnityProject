using UnityEngine;
using XNode;

/// <summary>外部参数-物体（不序列化；fallback 留空，运行期由外部注入）</summary>
[CreateNodeMenu("参数/输入/物体")]
public class ExternalParamGameObjectNode : ExternalParamNode<GameObject>
{
}