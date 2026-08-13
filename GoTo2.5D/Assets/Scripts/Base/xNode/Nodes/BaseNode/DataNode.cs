using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public abstract class DataNode : BaseNode
{
    public abstract override object GetValue(NodePort port);

    public override BaseNode GetConnectedNode()
    {
        return null;
    }

    public override void Execute()
    {
        return;
    }
}
