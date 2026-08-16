using UnityEngine;
using XNode;

/// <summary>
/// 生成-生成物体：实例化预制体。
/// - 位置：接线 > 节点上填写（非零）> 预制体原位置；
/// - 父物体：可选 GameObject 输入端口（不序列化场景引用）；
/// - 生成后的物体写到 spawned 输出端口（非序列化），可接后续操作节点 / 销毁节点。
/// </summary>
[CreateNodeMenu("生成/生成物体")]
[NodeTint("#88CC44")]
public class SpawnObjectNode : FlowNode
{
    [Header("预制体（从项目拖入）")]
    public GameObject prefab;

    [Header("生成位置（接线优先；未接线非零用填写值；否则用预制体原位置）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 spawnPosition;

    [Header("生成旋转（欧拉角，规则同上）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    public Vector3 eulerAngles;

    [Header("父物体（可选）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject parent;

    [Header("生成后的物体（输出端口）")]
    [Output]
    [System.NonSerialized]
    public GameObject spawned;

    public override void Execute()
    {
        if (prefab == null)
        {
            NodeLog.Warning($"{GetType().Name}: 未指定预制体");
            return;
        }

        GameObject go = GameObject.Instantiate(prefab);

        if (GetPort(nameof(spawnPosition)).IsConnected)
        {
            go.transform.position = GetInputValue<Vector3>(nameof(spawnPosition), spawnPosition);
        }
        else if (spawnPosition != Vector3.zero)
        {
            go.transform.position = spawnPosition;
        }
        else
        {
            go.transform.position = prefab.transform.position;
        }

        if (GetPort(nameof(eulerAngles)).IsConnected)
        {
            go.transform.rotation = Quaternion.Euler(GetInputValue<Vector3>(nameof(eulerAngles), eulerAngles));
        }
        else if (eulerAngles != Vector3.zero)
        {
            go.transform.rotation = Quaternion.Euler(eulerAngles);
        }
        else
        {
            go.transform.rotation = prefab.transform.rotation;
        }

        GameObject parentObj = GetInputValue<GameObject>(nameof(parent), null);
        if (parentObj != null)
        {
            go.transform.SetParent(parentObj.transform, true);
        }

        spawned = go;
        NodeLog.Info($"{GetType().Name}: 已生成 '{go.name}' ({prefab.name})");
        base.Execute();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(spawnPosition))
            return GetInputValue<Vector3>(nameof(spawnPosition), spawnPosition);
        if (port.fieldName == nameof(eulerAngles))
            return GetInputValue<Vector3>(nameof(eulerAngles), eulerAngles);
        if (port.fieldName == nameof(parent))
            return GetInputValue<GameObject>(nameof(parent), null);
        if (port.fieldName == nameof(spawned))
            return spawned;
        return null;
    }
}