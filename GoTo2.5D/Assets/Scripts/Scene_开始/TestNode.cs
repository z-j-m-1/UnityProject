using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using XNode;

[CreateNodeMenu("Test/TestNode")]
public class TestNode : Node
{
    [Input(ShowBackingValue.Unconnected)] public Sprite inputValue;

    [Input(ShowBackingValue.Unconnected)] public GameObject inputValue2;
}
