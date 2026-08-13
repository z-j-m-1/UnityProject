using System;
using UnityEngine;

/// <summary>
/// 变量基类（非抽象）
/// </summary>
[Serializable]
public class Variable<T>
{
    [SerializeField] private string _name;
    [SerializeField] private T _value;

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    public T Value
    {
        get { return _value; }
        set { _value = value; }
    }

    public Variable() { }

    public Variable(string name, T value)
    {
        _name = name;
        _value = value;
    }
}

// 不需要子类了！
// StringVariable, BoolVariable, IntVariable 都不需要了