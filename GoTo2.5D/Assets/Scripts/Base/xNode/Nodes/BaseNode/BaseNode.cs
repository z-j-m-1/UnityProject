using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using XNode;

// 节点基类
// 节点基类
public abstract class BaseNode : Node
{

    // 子类必须重写，返回该节点连接的下一个节点
    public abstract BaseNode GetConnectedNode();
    
    // 子类必须重写执行逻辑
    public abstract void Execute();

    // 基类留空，子类可重写以消除编辑器警告
    public override object GetValue(NodePort port)
    {
        return null;
    }
}
