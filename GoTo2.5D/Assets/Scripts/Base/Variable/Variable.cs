using System;
using UnityEngine;

[Serializable]
public class Variable<T> : ISerializationCallbackReceiver
{
    [SerializeField] private string _name;
    [SerializeField] private T _value;
    [SerializeField] private string _guid;

    /// <summary>是否持久化：关闭后不进存档、读档不覆盖，每次开始游戏重置为初始值（默认开启）</summary>
    [Tooltip("关闭后该变量不进存档，每次开始游戏重置为初始值")]
    public bool persist = true;

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

    public string Guid
    {
        get
        {
            EnsureGuid();
            return _guid;
        }
    }

    private void EnsureGuid()
    {
        if (string.IsNullOrEmpty(_guid))
        {
            _guid = System.Guid.NewGuid().ToString(); // 使用完全限定名
            // 或者 _guid = Guid.NewGuid().ToString(); // 如果 using System; 存在
        }
    }

    public void OnBeforeSerialize()
    {
        EnsureGuid();
    }

    public void OnAfterDeserialize()
    {
        EnsureGuid();
    }

    public Variable()
    {
        EnsureGuid();
    }

    public Variable(string name, T value)
    {
        _name = name;
        _value = value;
        EnsureGuid();
    }

    public void RegenerateGuid()
    {
        _guid = System.Guid.NewGuid().ToString();
    }
}