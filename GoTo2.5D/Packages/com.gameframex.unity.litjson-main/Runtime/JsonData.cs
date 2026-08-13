#region Header

/**
 * JsonData.cs
 *   Generic type to hold JSON data (objects, arrays, and so on). This is
 *   the default type returned by JsonMapper.ToObject().
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using UnityEngine.Scripting;


namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 通用 JSON 数据容器，可持有对象、数组、布尔、数值或字符串等任意 JSON 节点。
    /// </summary>
    /// <remarks>
    /// Generic container that holds any JSON node (object, array, boolean, number, or string).
    /// This is the default type returned by <c>JsonMapper.ToObject()</c>.
    /// </remarks>
    public class JsonData : IJsonWrapper, IEquatable<JsonData>
    {
        #region Fields

        private IList<JsonData> inst_array;
        private bool inst_boolean;
        private double inst_double;
        private int inst_int;
        private long inst_long;
        private IDictionary<string, JsonData> inst_object;
        private string inst_string;
        private string json;
        private JsonType type;

        // Used to implement the IOrderedDictionary interface
        private IList<KeyValuePair<string, JsonData>> object_list;

        // Reusable writer + lock for ToJson(), mirrors JsonMapper.static_writer pattern
        private static readonly JsonWriter _writer = new();
        private static readonly object _writerLock = new();

        #endregion


        #region Properties

        /// <summary>
        /// 获取当前 JSON 数据包含的元素数量。
        /// </summary>
        /// <remarks>
        /// Gets the number of elements contained in the current JSON data.
        /// </remarks>
        /// <value>元素数量 / The number of elements</value>
        [Preserve]
        public int Count
        {
            get { return EnsureCollection().Count; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为数组类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds an array.
        /// </remarks>
        /// <value>如果为数组类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds an array; otherwise <c>false</c></value>
        [Preserve]
        public bool IsArray
        {
            get { return type == JsonType.Array; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为布尔类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a boolean.
        /// </remarks>
        /// <value>如果为布尔类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds a boolean; otherwise <c>false</c></value>
        [Preserve]
        public bool IsBoolean
        {
            get { return type == JsonType.Boolean; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为双精度浮点类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a double-precision floating-point number.
        /// </remarks>
        /// <value>如果为双精度浮点类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds a double; otherwise <c>false</c></value>
        [Preserve]
        public bool IsDouble
        {
            get { return type == JsonType.Double; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为 32 位整数类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a 32-bit integer.
        /// </remarks>
        /// <value>如果为 32 位整数类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds an int; otherwise <c>false</c></value>
        [Preserve]
        public bool IsInt
        {
            get { return type == JsonType.Int; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为 64 位长整数类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a 64-bit long integer.
        /// </remarks>
        /// <value>如果为 64 位长整数类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds a long; otherwise <c>false</c></value>
        [Preserve]
        public bool IsLong
        {
            get { return type == JsonType.Long; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为对象类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds an object.
        /// </remarks>
        /// <value>如果为对象类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds an object; otherwise <c>false</c></value>
        [Preserve]
        public bool IsObject
        {
            get { return type == JsonType.Object; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为字符串类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a string.
        /// </remarks>
        /// <value>如果为字符串类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds a string; otherwise <c>false</c></value>
        [Preserve]
        public bool IsString
        {
            get { return type == JsonType.String; }
        }

        /// <summary>
        /// 获取当前对象的所有键集合（仅当数据为对象类型时有效）。
        /// </summary>
        /// <remarks>
        /// Gets the collection of keys in the object. Only valid when the data holds an object.
        /// </remarks>
        /// <value>键集合 / The collection of keys</value>
        [Preserve]
        public ICollection<string> Keys
        {
            get
            {
                EnsureDictionary();
                return inst_object.Keys;
            }
        }

        /// <summary>
        /// 确定当前 JSON 对象是否包含指定键的元素。
        /// </summary>
        /// <remarks>
        /// Determines whether the JSON object contains an element with the specified key.
        /// </remarks>
        /// <param name="key">要定位的键 / The key to locate</param>
        /// <returns>如果包含该键则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the key exists; otherwise <c>false</c></returns>
        [Preserve]
        public bool ContainsKey(string key)
        {
            EnsureDictionary();
            return inst_object.ContainsKey(key);
        }

        #endregion


        #region ICollection Properties

        /// <summary>
        /// 获取包含在 <see cref="ICollection"/> 中的元素数量。
        /// </summary>
        /// <remarks>
        /// Gets the number of elements contained in the <see cref="ICollection"/>.
        /// </remarks>
        /// <value>元素数量 / The number of elements</value>
        int ICollection.Count
        {
            get { return Count; }
        }

        /// <summary>
        /// 获取一个值，指示对 <see cref="ICollection"/> 的访问是否同步（线程安全）。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized (thread safe).
        /// </remarks>
        /// <value>始终返回 <c>false</c> / Always returns <c>false</c></value>
        bool ICollection.IsSynchronized
        {
            get { return EnsureCollection().IsSynchronized; }
        }

        /// <summary>
        /// 获取可用于同步对 <see cref="ICollection"/> 访问的对象。
        /// </summary>
        /// <remarks>
        /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
        /// </remarks>
        /// <value>用于同步访问的对象 / An object that can be used to synchronize access</value>
        object ICollection.SyncRoot
        {
            get { return EnsureCollection().SyncRoot; }
        }

        #endregion


        #region IDictionary Properties

        /// <summary>
        /// 获取一个值，指示 <see cref="IDictionary"/> 是否具有固定大小。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the <see cref="IDictionary"/> has a fixed size.
        /// </remarks>
        /// <value>如果具有固定大小则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if fixed size; otherwise <c>false</c></value>
        bool IDictionary.IsFixedSize
        {
            get { return EnsureDictionary().IsFixedSize; }
        }

        /// <summary>
        /// 获取一个值，指示 <see cref="IDictionary"/> 是否只读。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the <see cref="IDictionary"/> is read-only.
        /// </remarks>
        /// <value>如果只读则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if read-only; otherwise <c>false</c></value>
        bool IDictionary.IsReadOnly
        {
            get { return EnsureDictionary().IsReadOnly; }
        }

        /// <summary>
        /// 获取包含 <see cref="IDictionary"/> 中键的 <see cref="ICollection"/>。
        /// </summary>
        /// <remarks>
        /// Gets an <see cref="ICollection"/> containing the keys of the <see cref="IDictionary"/>.
        /// </remarks>
        /// <value>包含所有键的集合 / A collection containing all keys</value>
        ICollection IDictionary.Keys
        {
            get
            {
                EnsureDictionary();
                IList<string> keys = new List<string>();

                foreach (var entry in
                         object_list)
                {
                    keys.Add(entry.Key);
                }

                return (ICollection)keys;
            }
        }

        /// <summary>
        /// 获取包含 <see cref="IDictionary"/> 中值的 <see cref="ICollection"/>。
        /// </summary>
        /// <remarks>
        /// Gets an <see cref="ICollection"/> containing the values of the <see cref="IDictionary"/>.
        /// </remarks>
        /// <value>包含所有值的集合 / A collection containing all values</value>
        ICollection IDictionary.Values
        {
            get
            {
                EnsureDictionary();
                IList<JsonData> values = new List<JsonData>();

                foreach (var entry in
                         object_list)
                {
                    values.Add(entry.Value);
                }

                return (ICollection)values;
            }
        }

        #endregion


        #region IJsonWrapper Properties

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为数组类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds an array.
        /// </remarks>
        /// <value>如果为数组类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds an array; otherwise <c>false</c></value>
        bool IJsonWrapper.IsArray
        {
            get { return IsArray; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为布尔类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a boolean.
        /// </remarks>
        /// <value>如果为布尔类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds a boolean; otherwise <c>false</c></value>
        bool IJsonWrapper.IsBoolean
        {
            get { return IsBoolean; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为双精度浮点类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a double-precision floating-point number.
        /// </remarks>
        /// <value>如果为双精度浮点类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds a double; otherwise <c>false</c></value>
        bool IJsonWrapper.IsDouble
        {
            get { return IsDouble; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为 32 位整数类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a 32-bit integer.
        /// </remarks>
        /// <value>如果为 32 位整数类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds an int; otherwise <c>false</c></value>
        bool IJsonWrapper.IsInt
        {
            get { return IsInt; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为 64 位长整数类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a 64-bit long integer.
        /// </remarks>
        /// <value>如果为 64 位长整数类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds a long; otherwise <c>false</c></value>
        bool IJsonWrapper.IsLong
        {
            get { return IsLong; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为对象类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds an object.
        /// </remarks>
        /// <value>如果为对象类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds an object; otherwise <c>false</c></value>
        bool IJsonWrapper.IsObject
        {
            get { return IsObject; }
        }

        /// <summary>
        /// 获取一个值，指示当前 JSON 数据是否为字符串类型。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the current JSON data holds a string.
        /// </remarks>
        /// <value>如果为字符串类型则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the data holds a string; otherwise <c>false</c></value>
        bool IJsonWrapper.IsString
        {
            get { return IsString; }
        }

        #endregion


        #region IList Properties

        /// <summary>
        /// 获取一个值，指示 <see cref="IList"/> 是否具有固定大小。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the <see cref="IList"/> has a fixed size.
        /// </remarks>
        /// <value>如果具有固定大小则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if fixed size; otherwise <c>false</c></value>
        bool IList.IsFixedSize
        {
            get { return EnsureList().IsFixedSize; }
        }

        /// <summary>
        /// 获取一个值，指示 <see cref="IList"/> 是否只读。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the <see cref="IList"/> is read-only.
        /// </remarks>
        /// <value>如果只读则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if read-only; otherwise <c>false</c></value>
        bool IList.IsReadOnly
        {
            get { return EnsureList().IsReadOnly; }
        }

        #endregion


        #region IDictionary Indexer

        /// <summary>
        /// 获取或设置指定键处的元素。
        /// </summary>
        /// <remarks>
        /// Gets or sets the element with the specified key.
        /// </remarks>
        /// <param name="key">键对象，必须是 <see cref="String"/> 类型 / The key object; must be of type <see cref="String"/></param>
        /// <value>与指定键关联的元素 / The element associated with the specified key</value>
        /// <exception cref="ArgumentException">当 <paramref name="key"/> 不是字符串时抛出 / Thrown when <paramref name="key"/> is not a string</exception>
        object IDictionary.this[object key]
        {
            get { return EnsureDictionary()[key]; }

            set
            {
                if (!(key is string))
                {
                    throw new ArgumentException(
                        "The key has to be a string");
                }

                var data = ToJsonData(value);

                this[(string)key] = data;
            }
        }

        #endregion


        #region IOrderedDictionary Indexer

        /// <summary>
        /// 获取或设置指定索引处的元素。
        /// </summary>
        /// <remarks>
        /// Gets or sets the element at the specified index.
        /// </remarks>
        /// <param name="idx">从零开始的索引 / The zero-based index</param>
        /// <value>指定索引处的元素 / The element at the specified index</value>
        object IOrderedDictionary.this[int idx]
        {
            get
            {
                EnsureDictionary();
                return object_list[idx].Value;
            }

            set
            {
                EnsureDictionary();
                var data = ToJsonData(value);

                var old_entry = object_list[idx];

                inst_object[old_entry.Key] = data;

                var entry =
                    new KeyValuePair<string, JsonData>(old_entry.Key, data);

                object_list[idx] = entry;
            }
        }

        #endregion


        #region IList Indexer

        /// <summary>
        /// 获取或设置指定索引处的元素。
        /// </summary>
        /// <remarks>
        /// Gets or sets the element at the specified index.
        /// </remarks>
        /// <param name="index">从零开始的索引 / The zero-based index</param>
        /// <value>指定索引处的元素 / The element at the specified index</value>
        object IList.this[int index]
        {
            get { return EnsureList()[index]; }

            set
            {
                EnsureList();
                var data = ToJsonData(value);

                this[index] = data;
            }
        }

        #endregion


        #region Public Indexers

        /// <summary>
        /// 按属性名获取或设置对象的成员。
        /// </summary>
        /// <remarks>
        /// Gets or sets the member of the object by property name.
        /// </remarks>
        /// <param name="prop_name">属性名 / The property name</param>
        /// <value>与指定键关联的 <see cref="JsonData"/> 值 / The <see cref="JsonData"/> value associated with the specified key</value>
        [Preserve]
        public JsonData this[string prop_name]
        {
            get
            {
                EnsureDictionary();
                return inst_object[prop_name];
            }

            set
            {
                EnsureDictionary();

                var entry =
                    new KeyValuePair<string, JsonData>(prop_name, value);

                if (inst_object.ContainsKey(prop_name))
                {
                    for (var i = 0; i < object_list.Count; i++)
                    {
                        if (object_list[i].Key == prop_name)
                        {
                            object_list[i] = entry;
                            break;
                        }
                    }
                }
                else
                {
                    object_list.Add(entry);
                }

                inst_object[prop_name] = value;

                json = null;
            }
        }

        /// <summary>
        /// 按索引获取或设置数组元素或对象成员。
        /// </summary>
        /// <remarks>
        /// Gets or sets an array element or object member by index.
        /// </remarks>
        /// <param name="index">从零开始的索引 / The zero-based index</param>
        /// <value>指定索引处的 <see cref="JsonData"/> 值 / The <see cref="JsonData"/> value at the specified index</value>
        [Preserve]
        public JsonData this[int index]
        {
            get
            {
                EnsureCollection();

                if (type == JsonType.Array)
                {
                    return inst_array[index];
                }

                return object_list[index].Value;
            }

            set
            {
                EnsureCollection();

                if (type == JsonType.Array)
                {
                    inst_array[index] = value;
                }
                else
                {
                    var entry = object_list[index];
                    var new_entry =
                        new KeyValuePair<string, JsonData>(entry.Key, value);

                    object_list[index] = new_entry;
                    inst_object[entry.Key] = value;
                }

                json = null;
            }
        }

        #endregion


        #region Constructors

        /// <summary>
        /// 初始化 <see cref="JsonData"/> 类的新实例，数据类型为 <see cref="JsonType.None"/>。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonData"/> class with a JSON type of <see cref="JsonType.None"/>.
        /// </remarks>
        [Preserve]
        public JsonData()
        {
        }

        /// <summary>
        /// 用指定的布尔值初始化 <see cref="JsonData"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonData"/> class with the specified boolean value.
        /// </remarks>
        /// <param name="boolean">布尔值 / The boolean value</param>
        [Preserve]
        public JsonData(bool boolean)
        {
            type = JsonType.Boolean;
            inst_boolean = boolean;
        }

        /// <summary>
        /// 用指定的双精度浮点数初始化 <see cref="JsonData"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonData"/> class with the specified double-precision floating-point number.
        /// </remarks>
        /// <param name="number">双精度浮点数 / The double-precision floating-point number</param>
        [Preserve]
        public JsonData(double number)
        {
            type = JsonType.Double;
            inst_double = number;
        }

        /// <summary>
        /// 用指定的 32 位整数初始化 <see cref="JsonData"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonData"/> class with the specified 32-bit integer.
        /// </remarks>
        /// <param name="number">32 位整数 / The 32-bit integer</param>
        [Preserve]
        public JsonData(int number)
        {
            type = JsonType.Int;
            inst_int = number;
        }

        /// <summary>
        /// 用指定的 64 位长整数初始化 <see cref="JsonData"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonData"/> class with the specified 64-bit long integer.
        /// </remarks>
        /// <param name="number">64 位长整数 / The 64-bit long integer</param>
        [Preserve]
        public JsonData(long number)
        {
            type = JsonType.Long;
            inst_long = number;
        }

        /// <summary>
        /// 用指定对象初始化 <see cref="JsonData"/> 类的新实例，根据对象运行时类型自动推断 JSON 数据类型。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonData"/> class with the specified object,
        /// inferring the JSON data type from the runtime type of the object.
        /// </remarks>
        /// <param name="obj">要包装的对象（支持 <see cref="Boolean"/>、<see cref="Double"/>、<see cref="Int32"/>、<see cref="Int64"/>、<see cref="String"/>） / The object to wrap (supports <see cref="Boolean"/>, <see cref="Double"/>, <see cref="Int32"/>, <see cref="Int64"/>, <see cref="String"/>)</param>
        /// <exception cref="ArgumentException">当对象类型无法被包装时抛出 / Thrown when the object type cannot be wrapped</exception>
        [Preserve]
        public JsonData(object obj)
        {
            if (obj is bool)
            {
                type = JsonType.Boolean;
                inst_boolean = (bool)obj;
                return;
            }

            if (obj is double)
            {
                type = JsonType.Double;
                inst_double = (double)obj;
                return;
            }

            if (obj is int)
            {
                type = JsonType.Int;
                inst_int = (int)obj;
                return;
            }

            if (obj is long)
            {
                type = JsonType.Long;
                inst_long = (long)obj;
                return;
            }

            if (obj is string)
            {
                type = JsonType.String;
                inst_string = (string)obj;
                return;
            }

            throw new ArgumentException(
                "Unable to wrap the given object with JsonData");
        }

        /// <summary>
        /// 用指定的字符串初始化 <see cref="JsonData"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonData"/> class with the specified string.
        /// </remarks>
        /// <param name="str">字符串 / The string</param>
        [Preserve]
        public JsonData(string str)
        {
            type = JsonType.String;
            inst_string = str;
        }

        #endregion


        #region Implicit Conversions

        /// <summary>
        /// 将 <see cref="Boolean"/> 隐式转换为 <see cref="JsonData"/>。
        /// </summary>
        /// <remarks>
        /// Implicitly converts a <see cref="Boolean"/> to a <see cref="JsonData"/>.
        /// </remarks>
        /// <param name="data">要转换的布尔值 / The boolean value to convert</param>
        /// <returns>包含该布尔值的 <see cref="JsonData"/> 实例 / A <see cref="JsonData"/> instance holding the boolean value</returns>
        [Preserve]
        public static implicit operator JsonData(bool data)
        {
            return new JsonData(data);
        }

        /// <summary>
        /// 将 <see cref="Double"/> 隐式转换为 <see cref="JsonData"/>。
        /// </summary>
        /// <remarks>
        /// Implicitly converts a <see cref="Double"/> to a <see cref="JsonData"/>.
        /// </remarks>
        /// <param name="data">要转换的双精度浮点数 / The double value to convert</param>
        /// <returns>包含该双精度浮点数的 <see cref="JsonData"/> 实例 / A <see cref="JsonData"/> instance holding the double value</returns>
        [Preserve]
        public static implicit operator JsonData(double data)
        {
            return new JsonData(data);
        }

        /// <summary>
        /// 将 <see cref="Int32"/> 隐式转换为 <see cref="JsonData"/>。
        /// </summary>
        /// <remarks>
        /// Implicitly converts an <see cref="Int32"/> to a <see cref="JsonData"/>.
        /// </remarks>
        /// <param name="data">要转换的 32 位整数 / The 32-bit integer to convert</param>
        /// <returns>包含该整数的 <see cref="JsonData"/> 实例 / A <see cref="JsonData"/> instance holding the integer</returns>
        [Preserve]
        public static implicit operator JsonData(int data)
        {
            return new JsonData(data);
        }

        /// <summary>
        /// 将 <see cref="Int64"/> 隐式转换为 <see cref="JsonData"/>。
        /// </summary>
        /// <remarks>
        /// Implicitly converts an <see cref="Int64"/> to a <see cref="JsonData"/>.
        /// </remarks>
        /// <param name="data">要转换的 64 位长整数 / The 64-bit long integer to convert</param>
        /// <returns>包含该长整数的 <see cref="JsonData"/> 实例 / A <see cref="JsonData"/> instance holding the long integer</returns>
        [Preserve]
        public static implicit operator JsonData(long data)
        {
            return new JsonData(data);
        }

        /// <summary>
        /// 将 <see cref="String"/> 隐式转换为 <see cref="JsonData"/>。
        /// </summary>
        /// <remarks>
        /// Implicitly converts a <see cref="String"/> to a <see cref="JsonData"/>.
        /// </remarks>
        /// <param name="data">要转换的字符串 / The string to convert</param>
        /// <returns>包含该字符串的 <see cref="JsonData"/> 实例 / A <see cref="JsonData"/> instance holding the string</returns>
        [Preserve]
        public static implicit operator JsonData(string data)
        {
            return new JsonData(data);
        }

        #endregion


        #region Explicit Conversions

        /// <summary>
        /// 将 <see cref="JsonData"/> 显式转换为 <see cref="Boolean"/>。
        /// </summary>
        /// <remarks>
        /// Explicitly converts a <see cref="JsonData"/> to a <see cref="Boolean"/>.
        /// </remarks>
        /// <param name="data">要转换的 <see cref="JsonData"/> 实例 / The <see cref="JsonData"/> instance to convert</param>
        /// <returns>实例持有的布尔值 / The boolean value held by the instance</returns>
        /// <exception cref="InvalidCastException">当实例不持有布尔值时抛出 / Thrown when the instance does not hold a boolean</exception>
        [Preserve]
        public static explicit operator bool(JsonData data)
        {
            if (data.type != JsonType.Boolean)
            {
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a boolean");
            }

            return data.inst_boolean;
        }

        /// <summary>
        /// 将 <see cref="JsonData"/> 显式转换为 <see cref="Double"/>。
        /// </summary>
        /// <remarks>
        /// Explicitly converts a <see cref="JsonData"/> to a <see cref="Double"/>.
        /// </remarks>
        /// <param name="data">要转换的 <see cref="JsonData"/> 实例 / The <see cref="JsonData"/> instance to convert</param>
        /// <returns>实例持有的双精度浮点数 / The double value held by the instance</returns>
        /// <exception cref="InvalidCastException">当实例不持有双精度浮点数时抛出 / Thrown when the instance does not hold a double</exception>
        [Preserve]
        public static explicit operator double(JsonData data)
        {
            if (data.type != JsonType.Double)
            {
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a double");
            }

            return data.inst_double;
        }

        /// <summary>
        /// 将 <see cref="JsonData"/> 显式转换为 <see cref="Int32"/>。
        /// </summary>
        /// <remarks>
        /// Explicitly converts a <see cref="JsonData"/> to an <see cref="Int32"/>.
        /// When the instance holds an <see cref="Int64"/>, the value may be truncated.
        /// </remarks>
        /// <param name="data">要转换的 <see cref="JsonData"/> 实例 / The <see cref="JsonData"/> instance to convert</param>
        /// <returns>实例持有的 32 位整数值 / The 32-bit integer value held by the instance</returns>
        /// <exception cref="InvalidCastException">当实例既不持有 32 位整数也不持有 64 位长整数时抛出 / Thrown when the instance holds neither an int nor a long</exception>
        [Preserve]
        public static explicit operator int(JsonData data)
        {
            if (data.type != JsonType.Int && data.type != JsonType.Long)
            {
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold an int");
            }

            // cast may truncate data... but that's up to the user to consider
            return data.type == JsonType.Int ? data.inst_int : (int)data.inst_long;
        }

        /// <summary>
        /// 将 <see cref="JsonData"/> 显式转换为 <see cref="Int64"/>。
        /// </summary>
        /// <remarks>
        /// Explicitly converts a <see cref="JsonData"/> to an <see cref="Int64"/>.
        /// </remarks>
        /// <param name="data">要转换的 <see cref="JsonData"/> 实例 / The <see cref="JsonData"/> instance to convert</param>
        /// <returns>实例持有的 64 位长整数值 / The 64-bit long integer value held by the instance</returns>
        /// <exception cref="InvalidCastException">当实例既不持有 64 位长整数也不持有 32 位整数时抛出 / Thrown when the instance holds neither a long nor an int</exception>
        [Preserve]
        public static explicit operator long(JsonData data)
        {
            if (data.type != JsonType.Long && data.type != JsonType.Int)
            {
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a long");
            }

            return data.type == JsonType.Long ? data.inst_long : data.inst_int;
        }

        /// <summary>
        /// 将 <see cref="JsonData"/> 显式转换为 <see cref="String"/>。
        /// </summary>
        /// <remarks>
        /// Explicitly converts a <see cref="JsonData"/> to a <see cref="String"/>.
        /// </remarks>
        /// <param name="data">要转换的 <see cref="JsonData"/> 实例 / The <see cref="JsonData"/> instance to convert</param>
        /// <returns>实例持有的字符串 / The string held by the instance</returns>
        /// <exception cref="InvalidCastException">当实例不持有字符串时抛出 / Thrown when the instance does not hold a string</exception>
        [Preserve]
        public static explicit operator string(JsonData data)
        {
            if (data.type != JsonType.String)
            {
                throw new InvalidCastException(
                    "Instance of JsonData doesn't hold a string");
            }

            return data.inst_string;
        }

        #endregion


        #region ICollection Methods

        /// <summary>
        /// 从指定索引开始，将 <see cref="ICollection"/> 的元素复制到 <see cref="Array"/>。
        /// </summary>
        /// <remarks>
        /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>, starting at a particular index.
        /// </remarks>
        /// <param name="array">目标一维数组 / The one-dimensional destination array</param>
        /// <param name="index">目标数组中开始复制的从零开始的索引 / The zero-based index in the array at which copying begins</param>
        void ICollection.CopyTo(Array array, int index)
        {
            EnsureCollection().CopyTo(array, index);
        }

        #endregion


        #region IDictionary Methods

        /// <summary>
        /// 在 <see cref="IDictionary"/> 中添加带有所提供键和值的元素。
        /// </summary>
        /// <remarks>
        /// Adds an element with the provided key and value to the <see cref="IDictionary"/>.
        /// </remarks>
        /// <param name="key">作为要添加元素的键的对象（必须是字符串） / The object to use as the key of the element to add (must be a string)</param>
        /// <param name="value">作为要添加元素的值的对象 / The object to use as the value of the element to add</param>
        void IDictionary.Add(object key, object value)
        {
            var data = ToJsonData(value);

            EnsureDictionary().Add(key, data);

            var entry =
                new KeyValuePair<string, JsonData>((string)key, data);
            object_list.Add(entry);

            json = null;
        }

        /// <summary>
        /// 从 <see cref="IDictionary"/> 中移除所有元素。
        /// </summary>
        /// <remarks>
        /// Removes all elements from the <see cref="IDictionary"/>.
        /// </remarks>
        void IDictionary.Clear()
        {
            EnsureDictionary().Clear();
            object_list.Clear();
            json = null;
        }

        /// <summary>
        /// 确定 <see cref="IDictionary"/> 是否包含带有指定键的元素。
        /// </summary>
        /// <remarks>
        /// Determines whether the <see cref="IDictionary"/> contains an element with the specified key.
        /// </remarks>
        /// <param name="key">要在 <see cref="IDictionary"/> 中定位的键 / The key to locate in the <see cref="IDictionary"/></param>
        /// <returns>如果包含带指定键的元素则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the element exists; otherwise <c>false</c></returns>
        bool IDictionary.Contains(object key)
        {
            return EnsureDictionary().Contains(key);
        }

        /// <summary>
        /// 返回 <see cref="IDictionary"/> 的 <see cref="IDictionaryEnumerator"/>。
        /// </summary>
        /// <remarks>
        /// Returns an <see cref="IDictionaryEnumerator"/> for the <see cref="IDictionary"/>.
        /// </remarks>
        /// <returns>用于 <see cref="IDictionary"/> 的枚举器 / An enumerator for the <see cref="IDictionary"/></returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IOrderedDictionary)this).GetEnumerator();
        }

        /// <summary>
        /// 从 <see cref="IDictionary"/> 中移除带有指定键的元素。
        /// </summary>
        /// <remarks>
        /// Removes the element with the specified key from the <see cref="IDictionary"/>.
        /// </remarks>
        /// <param name="key">要移除的元素的键 / The key of the element to remove</param>
        void IDictionary.Remove(object key)
        {
            EnsureDictionary().Remove(key);

            for (var i = 0; i < object_list.Count; i++)
            {
                if (object_list[i].Key == (string)key)
                {
                    object_list.RemoveAt(i);
                    break;
                }
            }

            json = null;
        }

        #endregion


        #region IEnumerable Methods

        /// <summary>
        /// 返回一个循环访问集合的枚举器。
        /// </summary>
        /// <remarks>
        /// Returns an enumerator that iterates through the collection.
        /// </remarks>
        /// <returns>可用于循环访问集合的 <see cref="IEnumerator"/> / An <see cref="IEnumerator"/> that can be used to iterate through the collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnsureCollection().GetEnumerator();
        }

        #endregion


        #region IJsonWrapper Methods

        /// <summary>
        /// 返回当前实例持有的布尔值。
        /// </summary>
        /// <remarks>
        /// Returns the boolean value held by the current instance.
        /// </remarks>
        /// <returns>实例持有的布尔值 / The boolean value held by the instance</returns>
        /// <exception cref="InvalidOperationException">当实例不持有布尔值时抛出 / Thrown when the instance does not hold a boolean</exception>
        bool IJsonWrapper.GetBoolean()
        {
            if (type != JsonType.Boolean)
            {
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a boolean");
            }

            return inst_boolean;
        }

        /// <summary>
        /// 返回当前实例持有的双精度浮点数。
        /// </summary>
        /// <remarks>
        /// Returns the double-precision floating-point number held by the current instance.
        /// </remarks>
        /// <returns>实例持有的双精度浮点数 / The double value held by the instance</returns>
        /// <exception cref="InvalidOperationException">当实例不持有双精度浮点数时抛出 / Thrown when the instance does not hold a double</exception>
        double IJsonWrapper.GetDouble()
        {
            if (type != JsonType.Double)
            {
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a double");
            }

            return inst_double;
        }

        /// <summary>
        /// 返回当前实例持有的 32 位整数。
        /// </summary>
        /// <remarks>
        /// Returns the 32-bit integer held by the current instance.
        /// </remarks>
        /// <returns>实例持有的 32 位整数 / The int value held by the instance</returns>
        /// <exception cref="InvalidOperationException">当实例不持有 32 位整数时抛出 / Thrown when the instance does not hold an int</exception>
        int IJsonWrapper.GetInt()
        {
            if (type != JsonType.Int)
            {
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold an int");
            }

            return inst_int;
        }

        /// <summary>
        /// 返回当前实例持有的 64 位长整数。
        /// </summary>
        /// <remarks>
        /// Returns the 64-bit long integer held by the current instance.
        /// </remarks>
        /// <returns>实例持有的 64 位长整数 / The long value held by the instance</returns>
        /// <exception cref="InvalidOperationException">当实例不持有 64 位长整数时抛出 / Thrown when the instance does not hold a long</exception>
        long IJsonWrapper.GetLong()
        {
            if (type != JsonType.Long)
            {
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a long");
            }

            return inst_long;
        }

        /// <summary>
        /// 返回当前实例持有的字符串。
        /// </summary>
        /// <remarks>
        /// Returns the string held by the current instance.
        /// </remarks>
        /// <returns>实例持有的字符串 / The string held by the instance</returns>
        /// <exception cref="InvalidOperationException">当实例不持有字符串时抛出 / Thrown when the instance does not hold a string</exception>
        string IJsonWrapper.GetString()
        {
            if (type != JsonType.String)
            {
                throw new InvalidOperationException(
                    "JsonData instance doesn't hold a string");
            }

            return inst_string;
        }

        /// <summary>
        /// 将当前实例设置为持有指定的布尔值。
        /// </summary>
        /// <remarks>
        /// Sets the current instance to hold the specified boolean value.
        /// </remarks>
        /// <param name="val">要设置的布尔值 / The boolean value to set</param>
        void IJsonWrapper.SetBoolean(bool val)
        {
            type = JsonType.Boolean;
            inst_boolean = val;
            json = null;
        }

        /// <summary>
        /// 将当前实例设置为持有指定的双精度浮点数。
        /// </summary>
        /// <remarks>
        /// Sets the current instance to hold the specified double-precision floating-point number.
        /// </remarks>
        /// <param name="val">要设置的双精度浮点数 / The double value to set</param>
        void IJsonWrapper.SetDouble(double val)
        {
            type = JsonType.Double;
            inst_double = val;
            json = null;
        }

        /// <summary>
        /// 将当前实例设置为持有指定的 32 位整数。
        /// </summary>
        /// <remarks>
        /// Sets the current instance to hold the specified 32-bit integer.
        /// </remarks>
        /// <param name="val">要设置的 32 位整数 / The int value to set</param>
        void IJsonWrapper.SetInt(int val)
        {
            type = JsonType.Int;
            inst_int = val;
            json = null;
        }

        /// <summary>
        /// 将当前实例设置为持有指定的 64 位长整数。
        /// </summary>
        /// <remarks>
        /// Sets the current instance to hold the specified 64-bit long integer.
        /// </remarks>
        /// <param name="val">要设置的 64 位长整数 / The long value to set</param>
        void IJsonWrapper.SetLong(long val)
        {
            type = JsonType.Long;
            inst_long = val;
            json = null;
        }

        /// <summary>
        /// 将当前实例设置为持有指定的字符串。
        /// </summary>
        /// <remarks>
        /// Sets the current instance to hold the specified string.
        /// </remarks>
        /// <param name="val">要设置的字符串 / The string to set</param>
        void IJsonWrapper.SetString(string val)
        {
            type = JsonType.String;
            inst_string = val;
            json = null;
        }

        /// <summary>
        /// 将当前实例序列化为 JSON 字符串。
        /// </summary>
        /// <remarks>
        /// Serializes the current instance to a JSON string.
        /// </remarks>
        /// <returns>序列化后的 JSON 字符串 / The serialized JSON string</returns>
        string IJsonWrapper.ToJson()
        {
            return ToJson();
        }

        /// <summary>
        /// 将当前实例序列化到指定的 <see cref="JsonWriter"/>。
        /// </summary>
        /// <remarks>
        /// Serializes the current instance to the specified <see cref="JsonWriter"/>.
        /// </remarks>
        /// <param name="writer">目标 <see cref="JsonWriter"/> / The target <see cref="JsonWriter"/></param>
        void IJsonWrapper.ToJson(JsonWriter writer)
        {
            ToJson(writer);
        }

        #endregion


        #region IList Methods

        /// <summary>
        /// 将指定对象添加到 <see cref="IList"/> 中。
        /// </summary>
        /// <remarks>
        /// Adds the specified object to the <see cref="IList"/>.
        /// </remarks>
        /// <param name="value">要添加到 <see cref="IList"/> 的对象 / The object to add to the <see cref="IList"/></param>
        /// <returns>新元素插入位置的从零开始的索引 / The zero-based index at which the new element was inserted</returns>
        int IList.Add(object value)
        {
            return Add(value);
        }

        /// <summary>
        /// 从 <see cref="IList"/> 中移除所有项。
        /// </summary>
        /// <remarks>
        /// Removes all items from the <see cref="IList"/>.
        /// </remarks>
        void IList.Clear()
        {
            EnsureList().Clear();
            json = null;
        }

        /// <summary>
        /// 确定 <see cref="IList"/> 是否包含特定值。
        /// </summary>
        /// <remarks>
        /// Determines whether the <see cref="IList"/> contains a specific value.
        /// </remarks>
        /// <param name="value">要在 <see cref="IList"/> 中定位的对象 / The object to locate in the <see cref="IList"/></param>
        /// <returns>如果在 <see cref="IList"/> 中找到该项则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if found; otherwise <c>false</c></returns>
        bool IList.Contains(object value)
        {
            return EnsureList().Contains(value);
        }

        /// <summary>
        /// 确定 <see cref="IList"/> 中特定项的索引。
        /// </summary>
        /// <remarks>
        /// Determines the index of a specific item in the <see cref="IList"/>.
        /// </remarks>
        /// <param name="value">要在 <see cref="IList"/> 中定位的对象 / The object to locate in the <see cref="IList"/></param>
        /// <returns>如果在列表中找到则返回该项的索引；否则返回 -1 / The index of the item if found; otherwise -1</returns>
        int IList.IndexOf(object value)
        {
            return EnsureList().IndexOf(value);
        }

        /// <summary>
        /// 在指定索引处将一个项插入到 <see cref="IList"/> 中。
        /// </summary>
        /// <remarks>
        /// Inserts an item to the <see cref="IList"/> at the specified index.
        /// </remarks>
        /// <param name="index">从零开始的插入索引 / The zero-based index at which to insert</param>
        /// <param name="value">要插入到 <see cref="IList"/> 中的对象 / The object to insert into the <see cref="IList"/></param>
        void IList.Insert(int index, object value)
        {
            EnsureList().Insert(index, value);
            json = null;
        }

        /// <summary>
        /// 从 <see cref="IList"/> 中移除特定对象的第一个匹配项。
        /// </summary>
        /// <remarks>
        /// Removes the first occurrence of a specific object from the <see cref="IList"/>.
        /// </remarks>
        /// <param name="value">要从 <see cref="IList"/> 中移除的对象 / The object to remove from the <see cref="IList"/></param>
        void IList.Remove(object value)
        {
            EnsureList().Remove(value);
            json = null;
        }

        /// <summary>
        /// 移除指定索引处的 <see cref="IList"/> 项。
        /// </summary>
        /// <remarks>
        /// Removes the <see cref="IList"/> item at the specified index.
        /// </remarks>
        /// <param name="index">要移除项的从零开始的索引 / The zero-based index of the item to remove</param>
        void IList.RemoveAt(int index)
        {
            EnsureList().RemoveAt(index);
            json = null;
        }

        #endregion


        #region IOrderedDictionary Methods

        /// <summary>
        /// 返回一个循环访问 <see cref="IOrderedDictionary"/> 集合的 <see cref="IDictionaryEnumerator"/>。
        /// </summary>
        /// <remarks>
        /// Returns an <see cref="IDictionaryEnumerator"/> that iterates through the <see cref="IOrderedDictionary"/> collection.
        /// </remarks>
        /// <returns>用于 <see cref="IOrderedDictionary"/> 集合的枚举器 / An enumerator for the <see cref="IOrderedDictionary"/> collection</returns>
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            EnsureDictionary();

            return new OrderedDictionaryEnumerator(
                object_list.GetEnumerator());
        }

        /// <summary>
        /// 在指定索引处将带有提供键和值的元素插入到 <see cref="IOrderedDictionary"/> 集合中。
        /// </summary>
        /// <remarks>
        /// Inserts an element with the provided key and value into the <see cref="IOrderedDictionary"/> collection at the specified index.
        /// </remarks>
        /// <param name="idx">从零开始的插入索引 / The zero-based index at which to insert</param>
        /// <param name="key">作为要添加元素的键的对象（必须是字符串） / The object to use as the key of the element to add (must be a string)</param>
        /// <param name="value">作为要添加元素的值的对象 / The object to use as the value of the element to add</param>
        void IOrderedDictionary.Insert(int idx, object key, object value)
        {
            var property = (string)key;
            var data = ToJsonData(value);

            this[property] = data;

            var entry =
                new KeyValuePair<string, JsonData>(property, data);

            object_list.Insert(idx, entry);
        }

        /// <summary>
        /// 移除指定索引处的 <see cref="IOrderedDictionary"/> 元素。
        /// </summary>
        /// <remarks>
        /// Removes the <see cref="IOrderedDictionary"/> element at the specified index.
        /// </remarks>
        /// <param name="idx">要移除元素的从零开始的索引 / The zero-based index of the element to remove</param>
        void IOrderedDictionary.RemoveAt(int idx)
        {
            EnsureDictionary();

            inst_object.Remove(object_list[idx].Key);
            object_list.RemoveAt(idx);
        }

        #endregion


        #region Private Methods

        private ICollection EnsureCollection()
        {
            if (type == JsonType.Array)
            {
                return (ICollection)inst_array;
            }

            if (type == JsonType.Object)
            {
                return (ICollection)inst_object;
            }

            throw new InvalidOperationException(
                "The JsonData instance has to be initialized first");
        }

        private IDictionary EnsureDictionary()
        {
            if (type == JsonType.Object)
            {
                return (IDictionary)inst_object;
            }

            if (type != JsonType.None)
            {
                throw new InvalidOperationException(
                    "Instance of JsonData is not a dictionary");
            }

            type = JsonType.Object;
            inst_object = new Dictionary<string, JsonData>();
            object_list = new List<KeyValuePair<string, JsonData>>();

            return (IDictionary)inst_object;
        }

        private IList EnsureList()
        {
            if (type == JsonType.Array)
            {
                return (IList)inst_array;
            }

            if (type != JsonType.None)
            {
                throw new InvalidOperationException(
                    "Instance of JsonData is not a list");
            }

            type = JsonType.Array;
            inst_array = new List<JsonData>();

            return (IList)inst_array;
        }

        private JsonData ToJsonData(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is JsonData)
            {
                return (JsonData)obj;
            }

            return new JsonData(obj);
        }

        private static void WriteJson(IJsonWrapper obj, JsonWriter writer)
        {
            if (obj == null)
            {
                writer.Write(null);
                return;
            }

            if (obj.IsString)
            {
                writer.Write(obj.GetString());
                return;
            }

            if (obj.IsBoolean)
            {
                writer.Write(obj.GetBoolean());
                return;
            }

            if (obj.IsDouble)
            {
                writer.Write(obj.GetDouble());
                return;
            }

            if (obj.IsInt)
            {
                writer.Write(obj.GetInt());
                return;
            }

            if (obj.IsLong)
            {
                writer.Write(obj.GetLong());
                return;
            }

            if (obj.IsArray)
            {
                writer.WriteArrayStart();
                foreach (var elem in (IList)obj)
                {
                    WriteJson((JsonData)elem, writer);
                }

                writer.WriteArrayEnd();

                return;
            }

            if (obj.IsObject)
            {
                writer.WriteObjectStart();

                foreach (DictionaryEntry entry in (IDictionary)obj)
                {
                    writer.WritePropertyName((string)entry.Key);
                    WriteJson((JsonData)entry.Value, writer);
                }

                writer.WriteObjectEnd();

                return;
            }
        }

        #endregion


        /// <summary>
        /// 将指定对象添加到数组末尾，并返回新元素的索引。
        /// </summary>
        /// <remarks>
        /// Adds the specified object to the end of the array and returns the index of the newly added element.
        /// </remarks>
        /// <param name="value">要添加的对象 / The object to add</param>
        /// <returns>新添加元素的从零开始的索引 / The zero-based index of the newly added element</returns>
        [Preserve]
        public int Add(object value)
        {
            var data = ToJsonData(value);

            json = null;

            return EnsureList().Add(data);
        }

        /// <summary>
        /// 从对象中移除指定键的属性，或从数组中移除指定元素。
        /// </summary>
        /// <remarks>
        /// Removes the property with the specified key from the object, or removes the specified element from the array.
        /// </remarks>
        /// <param name="obj">对象模式下为键字符串，数组模式下为要移除的元素 / In object mode the key string; in array mode the element to remove</param>
        /// <returns>如果成功移除则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the element was removed; otherwise <c>false</c></returns>
        /// <exception cref="KeyNotFoundException">对象模式下当指定键不存在时抛出 / Thrown in object mode when the specified key is not found</exception>
        /// <exception cref="InvalidOperationException">当实例既不是对象也不是数组时抛出 / Thrown when the instance is neither an object nor an array</exception>
        [Preserve]
        public bool Remove(object obj)
        {
            json = null;
            if (IsObject)
            {
                if (!(obj is string key))
                {
                    return false;
                }

                JsonData value = null;
                if (inst_object.TryGetValue(key, out value))
                {
                    return inst_object.Remove(key) && object_list.Remove(new KeyValuePair<string, JsonData>(key, value));
                }
                else
                {
                    throw new KeyNotFoundException("The specified key was not found in the JsonData object.");
                }
            }

            if (IsArray)
            {
                return inst_array.Remove(ToJsonData(obj));
            }

            throw new InvalidOperationException(
                "Instance of JsonData is not an object or a list.");
        }

        /// <summary>
        /// 清空当前 JSON 数据中的所有元素。
        /// </summary>
        /// <remarks>
        /// Removes all elements from the current JSON data.
        /// </remarks>
        [Preserve]
        public void Clear()
        {
            if (IsObject)
            {
                ((IDictionary)this).Clear();
                return;
            }

            if (IsArray)
            {
                ((IList)this).Clear();
                return;
            }
        }

        /// <summary>
        /// 比较当前实例与指定的 <see cref="JsonData"/> 是否相等（按值比较，支持 int/long 等价判定）。
        /// </summary>
        /// <remarks>
        /// Compares the current instance with the specified <see cref="JsonData"/> for value equality
        /// (with int/long equivalence handling).
        /// </remarks>
        /// <param name="x">要比较的 <see cref="JsonData"/> 实例 / The <see cref="JsonData"/> instance to compare</param>
        /// <returns>如果两者相等则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if equal; otherwise <c>false</c></returns>
        [Preserve]
        public bool Equals(JsonData x)
        {
            if (x == null)
            {
                return false;
            }

            if (x.type != type)
            {
                // further check to see if this is a long to int comparison
                if ((x.type != JsonType.Int && x.type != JsonType.Long)
                    || (type != JsonType.Int && type != JsonType.Long))
                {
                    return false;
                }
            }

            switch (type)
            {
                case JsonType.None:
                    return true;

                case JsonType.Object:
                    if (x.inst_object == null || inst_object == null)
                    {
                        return x.inst_object == inst_object;
                    }

                    if (x.inst_object.Count != inst_object.Count)
                    {
                        return false;
                    }

                    foreach (var kvp in inst_object)
                    {
                        JsonData otherValue;
                        if (!x.inst_object.TryGetValue(kvp.Key, out otherValue))
                        {
                            return false;
                        }

                        if (!kvp.Value.Equals(otherValue))
                        {
                            return false;
                        }
                    }

                    return true;

                case JsonType.Array:
                    if (x.inst_array == null || inst_array == null)
                    {
                        return x.inst_array == inst_array;
                    }

                    if (x.inst_array.Count != inst_array.Count)
                    {
                        return false;
                    }

                    for (var i = 0; i < inst_array.Count; i++)
                    {
                        if (!inst_array[i].Equals(x.inst_array[i]))
                        {
                            return false;
                        }
                    }

                    return true;

                case JsonType.String:
                    return inst_string.Equals(x.inst_string);

                case JsonType.Int:
                {
                    if (x.IsLong)
                    {
                        if (x.inst_long < int.MinValue || x.inst_long > int.MaxValue)
                        {
                            return false;
                        }

                        return inst_int.Equals((int)x.inst_long);
                    }

                    return inst_int.Equals(x.inst_int);
                }

                case JsonType.Long:
                {
                    if (x.IsInt)
                    {
                        if (inst_long < int.MinValue || inst_long > int.MaxValue)
                        {
                            return false;
                        }

                        return x.inst_int.Equals((int)inst_long);
                    }

                    return inst_long.Equals(x.inst_long);
                }

                case JsonType.Double:
                    return inst_double.Equals(x.inst_double);

                case JsonType.Boolean:
                    return inst_boolean.Equals(x.inst_boolean);
            }

            return false;
        }

        /// <summary>
        /// 获取当前 JSON 数据的类型。
        /// </summary>
        /// <remarks>
        /// Gets the JSON data type of the current instance.
        /// </remarks>
        /// <returns>当前实例持有的 <see cref="JsonType"/> / The <see cref="JsonType"/> held by the current instance</returns>
        [Preserve]
        public JsonType GetJsonType()
        {
            return type;
        }

        /// <summary>
        /// 设置当前 JSON 数据的类型，并将对应的内部字段重置为默认值。
        /// </summary>
        /// <remarks>
        /// Sets the JSON data type of the current instance, resetting the corresponding internal fields to their default values.
        /// </remarks>
        /// <param name="type">目标 JSON 数据类型 / The target JSON data type</param>
        [Preserve]
        public void SetJsonType(JsonType type)
        {
            if (this.type == type)
            {
                return;
            }

            switch (type)
            {
                case JsonType.None:
                    break;

                case JsonType.Object:
                    inst_object = new Dictionary<string, JsonData>();
                    object_list = new List<KeyValuePair<string, JsonData>>();
                    break;

                case JsonType.Array:
                    inst_array = new List<JsonData>();
                    break;

                case JsonType.String:
                    inst_string = default;
                    break;

                case JsonType.Int:
                    inst_int = default;
                    break;

                case JsonType.Long:
                    inst_long = default;
                    break;

                case JsonType.Double:
                    inst_double = default;
                    break;

                case JsonType.Boolean:
                    inst_boolean = default;
                    break;
            }

            this.type = type;
        }

        /// <summary>
        /// 将当前 JSON 数据序列化为 JSON 字符串。
        /// </summary>
        /// <remarks>
        /// Serializes the current JSON data to a JSON string. The result is cached after the first call.
        /// </remarks>
        /// <returns>序列化后的 JSON 字符串 / The serialized JSON string</returns>
        [Preserve]
        public string ToJson()
        {
            if (json != null)
            {
                return json;
            }

            lock (_writerLock)
            {
                if (json != null)
                {
                    return json;
                }

                _writer.Reset();
                _writer.Validate = false;

                WriteJson(this, _writer);
                json = _writer.ToString();
            }

            return json;
        }

        /// <summary>
        /// 将当前 JSON 数据序列化到指定的 <see cref="JsonWriter"/>。
        /// </summary>
        /// <remarks>
        /// Serializes the current JSON data to the specified <see cref="JsonWriter"/>.
        /// </remarks>
        /// <param name="writer">目标 <see cref="JsonWriter"/> / The target <see cref="JsonWriter"/></param>
        [Preserve]
        public void ToJson(JsonWriter writer)
        {
            var old_validate = writer.Validate;

            writer.Validate = false;

            WriteJson(this, writer);

            writer.Validate = old_validate;
        }

        /// <summary>
        /// 返回当前 JSON 数据的字符串表示。
        /// </summary>
        /// <remarks>
        /// Returns a string representation of the current JSON data.
        /// For scalar types this returns the underlying value's string form;
        /// for objects and arrays this returns a type label.
        /// </remarks>
        /// <returns>表示当前 JSON 数据的字符串 / A string that represents the current JSON data</returns>
        public override string ToString()
        {
            switch (type)
            {
                case JsonType.Array:
                    return "JsonData array";

                case JsonType.Boolean:
                    return inst_boolean.ToString();

                case JsonType.Double:
                    return inst_double.ToString();

                case JsonType.Int:
                    return inst_int.ToString();

                case JsonType.Long:
                    return inst_long.ToString();

                case JsonType.Object:
                    return "JsonData object";

                case JsonType.String:
                    return inst_string;
            }

            return "Uninitialized JsonData";
        }
    }


    /// <summary>
    /// 内部枚举器，包装 <see cref="IEnumerator{T}"/> 以提供 <see cref="IDictionaryEnumerator"/> 语义，按插入顺序遍历 <see cref="JsonData"/> 对象成员。
    /// </summary>
    /// <remarks>
    /// Internal enumerator that wraps an <see cref="IEnumerator{T}"/> to provide <see cref="IDictionaryEnumerator"/> semantics,
    /// used to iterate over the object members of a <see cref="JsonData"/> in insertion order.
    /// </remarks>
    internal class OrderedDictionaryEnumerator : IDictionaryEnumerator
    {
        private IEnumerator<KeyValuePair<string, JsonData>> list_enumerator;


        /// <summary>
        /// 获取集合中当前位置的元素。
        /// </summary>
        /// <remarks>
        /// Gets the current element in the collection.
        /// </remarks>
        /// <value>集合中当前位置的元素 / The current element in the collection</value>
        public object Current
        {
            get { return Entry; }
        }

        /// <summary>
        /// 获取当前位置的键和值，以 <see cref="DictionaryEntry"/> 结构形式返回。
        /// </summary>
        /// <remarks>
        /// Gets both the key and the value of the current dictionary entry as a <see cref="DictionaryEntry"/> structure.
        /// </remarks>
        /// <value>当前字典条目 / The current dictionary entry</value>
        public DictionaryEntry Entry
        {
            get
            {
                var curr = list_enumerator.Current;
                return new DictionaryEntry(curr.Key, curr.Value);
            }
        }

        /// <summary>
        /// 获取当前字典条目的键。
        /// </summary>
        /// <remarks>
        /// Gets the key of the current dictionary entry.
        /// </remarks>
        /// <value>当前条目的键 / The key of the current entry</value>
        public object Key
        {
            get { return list_enumerator.Current.Key; }
        }

        /// <summary>
        /// 获取当前字典条目的值。
        /// </summary>
        /// <remarks>
        /// Gets the value of the current dictionary entry.
        /// </remarks>
        /// <value>当前条目的值 / The value of the current entry</value>
        public object Value
        {
            get { return list_enumerator.Current.Value; }
        }


        /// <summary>
        /// 用指定的键值对枚举器初始化 <see cref="OrderedDictionaryEnumerator"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="OrderedDictionaryEnumerator"/> class with the specified key-value pair enumerator.
        /// </remarks>
        /// <param name="enumerator">要包装的键值对枚举器 / The key-value pair enumerator to wrap</param>
        public OrderedDictionaryEnumerator(
            IEnumerator<KeyValuePair<string, JsonData>> enumerator)
        {
            list_enumerator = enumerator;
        }


        /// <summary>
        /// 将枚举器推进到集合的下一个元素。
        /// </summary>
        /// <remarks>
        /// Advances the enumerator to the next element of the collection.
        /// </remarks>
        /// <returns>如果枚举器成功推进到下一个元素则返回 <c>true</c>；如果枚举器已越过集合末尾则返回 <c>false</c> / <c>true</c> if advanced; <c>false</c> if past the end</returns>
        public bool MoveNext()
        {
            return list_enumerator.MoveNext();
        }

        /// <summary>
        /// 将枚举器设置到其初始位置，即集合中第一个元素之前。
        /// </summary>
        /// <remarks>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </remarks>
        public void Reset()
        {
            list_enumerator.Reset();
        }
    }
}