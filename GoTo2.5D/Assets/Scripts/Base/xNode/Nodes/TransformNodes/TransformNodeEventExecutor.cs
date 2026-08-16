using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformNodeEventExecutor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MoveObjectNodeEvent.Subscribe(MoveObject);
        RoteObjectNodeEvent.Subscribe(RoteObject);
    }

    private void MoveObject(MoveObjectNodeEvent moveObjectNodeEvent)
    {
        GameObject targetObject = moveObjectNodeEvent.targetObject;
        targetObject.transform.position += moveObjectNodeEvent.moveOffset;
        NodeLog.Info($"移动物体: {targetObject.name}, 移动偏移量: {moveObjectNodeEvent.moveOffset}");
    }

    private void RoteObject(RoteObjectNodeEvent roteObjectNodeEvent)
    {
        GameObject targetObject = roteObjectNodeEvent.targetObject;
        targetObject.transform.rotation *= Quaternion.Euler(roteObjectNodeEvent.roteOffset);
        NodeLog.Info($"旋转物体: {targetObject.name}, 旋转偏移量: {roteObjectNodeEvent.roteOffset}");
    }
}
