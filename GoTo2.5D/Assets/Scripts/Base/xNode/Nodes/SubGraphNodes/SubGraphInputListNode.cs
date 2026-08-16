using System;
using System.Collections.Generic;
using XNode;

/// <summary>
/// 子图参数输入-列表（泛型基类）：参数名 = 子图变量名，参数类型 = List&lt;T&gt;。
/// 具体类型子类各自成同名单文件（字符串/整数/浮点/二维向量/三维向量列表）。
/// </summary>
public abstract class SubGraphInputListNode<T> : SubGraphInputNode<List<T>>
{
    public override Type ParamType => typeof(List<T>);
}