#region Header

/**
 * JsonMockWrapper.cs
 *   Mock object implementing IJsonWrapper, to facilitate actions like
 *   skipping data more efficiently.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


using System;
using System.Collections;
using System.Collections.Specialized;
using UnityEngine.Scripting;


namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 实现 IJsonWrapper 的 mock 对象，用于跳过 JSON 数据，提高解析效率。
    /// 所有成员均为空实现或返回默认值，不保存任何实际数据。
    /// </summary>
    /// <remarks>
    /// Mock object implementing IJsonWrapper, to facilitate actions like
    /// skipping data more efficiently. All members are no-op stubs or return
    /// default values without storing any real data.
    /// </remarks>
    public class JsonMockWrapper : IJsonWrapper
    {
        /// <summary>
        /// 获取 mock 对象是否为数组，始终返回 false。
        /// </summary>
        /// <remarks>Gets whether this mock holds an array; always returns false.</remarks>
        /// <value>始终为 false / Always false.</value>
        [Preserve]
        public bool IsArray
        {
            get { return false; }
        }

        /// <summary>
        /// 获取 mock 对象是否为布尔值，始终返回 false。
        /// </summary>
        /// <remarks>Gets whether this mock holds a boolean; always returns false.</remarks>
        /// <value>始终为 false / Always false.</value>
        [Preserve]
        public bool IsBoolean
        {
            get { return false; }
        }

        /// <summary>
        /// 获取 mock 对象是否为双精度浮点数，始终返回 false。
        /// </summary>
        /// <remarks>Gets whether this mock holds a double; always returns false.</remarks>
        /// <value>始终为 false / Always false.</value>
        [Preserve]
        public bool IsDouble
        {
            get { return false; }
        }

        /// <summary>
        /// 获取 mock 对象是否为 32 位整数，始终返回 false。
        /// </summary>
        /// <remarks>Gets whether this mock holds an int; always returns false.</remarks>
        /// <value>始终为 false / Always false.</value>
        [Preserve]
        public bool IsInt
        {
            get { return false; }
        }

        /// <summary>
        /// 获取 mock 对象是否为 64 位长整数，始终返回 false。
        /// </summary>
        /// <remarks>Gets whether this mock holds a long; always returns false.</remarks>
        /// <value>始终为 false / Always false.</value>
        [Preserve]
        public bool IsLong
        {
            get { return false; }
        }

        /// <summary>
        /// 获取 mock 对象是否为对象，始终返回 false。
        /// </summary>
        /// <remarks>Gets whether this mock holds an object; always returns false.</remarks>
        /// <value>始终为 false / Always false.</value>
        [Preserve]
        public bool IsObject
        {
            get { return false; }
        }

        /// <summary>
        /// 获取 mock 对象是否为字符串，始终返回 false。
        /// </summary>
        /// <remarks>Gets whether this mock holds a string; always returns false.</remarks>
        /// <value>始终为 false / Always false.</value>
        [Preserve]
        public bool IsString
        {
            get { return false; }
        }

        /// <summary>
        /// 获取 mock 对象的布尔值，始终返回 false。
        /// </summary>
        /// <remarks>Gets the boolean value of this mock; always returns false.</remarks>
        /// <returns>始终为 false / Always false.</returns>
        [Preserve]
        public bool GetBoolean()
        {
            return false;
        }

        /// <summary>
        /// 获取 mock 对象的双精度浮点数，始终返回 0.0。
        /// </summary>
        /// <remarks>Gets the double value of this mock; always returns 0.0.</remarks>
        /// <returns>始终为 0.0 / Always 0.0.</returns>
        [Preserve]
        public double GetDouble()
        {
            return 0.0;
        }

        /// <summary>
        /// 获取 mock 对象的 32 位整数，始终返回 0。
        /// </summary>
        /// <remarks>Gets the int value of this mock; always returns 0.</remarks>
        /// <returns>始终为 0 / Always 0.</returns>
        [Preserve]
        public int GetInt()
        {
            return 0;
        }

        /// <summary>
        /// 获取 mock 对象的 JSON 数据类型，始终返回 JsonType.None。
        /// </summary>
        /// <remarks>Gets the JSON data type of this mock; always returns JsonType.None.</remarks>
        /// <returns>始终为 JsonType.None / Always JsonType.None.</returns>
        [Preserve]
        public JsonType GetJsonType()
        {
            return JsonType.None;
        }

        /// <summary>
        /// 获取 mock 对象的 64 位长整数，始终返回 0L。
        /// </summary>
        /// <remarks>Gets the long value of this mock; always returns 0L.</remarks>
        /// <returns>始终为 0L / Always 0L.</returns>
        [Preserve]
        public long GetLong()
        {
            return 0L;
        }

        /// <summary>
        /// 获取 mock 对象的字符串，始终返回空字符串。
        /// </summary>
        /// <remarks>Gets the string value of this mock; always returns an empty string.</remarks>
        /// <returns>始终为空字符串 / Always an empty string.</returns>
        [Preserve]
        public string GetString()
        {
            return "";
        }

        /// <summary>
        /// 设置 mock 对象的布尔值，空实现。
        /// </summary>
        /// <remarks>Sets the boolean value of this mock; no-op stub.</remarks>
        /// <param name="val">要设置的布尔值（被忽略）/ The boolean value to set (ignored).</param>
        [Preserve]
        public void SetBoolean(bool val)
        {
        }

        /// <summary>
        /// 设置 mock 对象的双精度浮点数，空实现。
        /// </summary>
        /// <remarks>Sets the double value of this mock; no-op stub.</remarks>
        /// <param name="val">要设置的双精度浮点数（被忽略）/ The double value to set (ignored).</param>
        [Preserve]
        public void SetDouble(double val)
        {
        }

        /// <summary>
        /// 设置 mock 对象的 32 位整数，空实现。
        /// </summary>
        /// <remarks>Sets the int value of this mock; no-op stub.</remarks>
        /// <param name="val">要设置的 32 位整数（被忽略）/ The int value to set (ignored).</param>
        [Preserve]
        public void SetInt(int val)
        {
        }

        /// <summary>
        /// 设置 mock 对象的 JSON 数据类型，空实现。
        /// </summary>
        /// <remarks>Sets the JSON data type of this mock; no-op stub.</remarks>
        /// <param name="type">要设置的 JSON 数据类型（被忽略）/ The JSON data type to set (ignored).</param>
        [Preserve]
        public void SetJsonType(JsonType type)
        {
        }

        /// <summary>
        /// 设置 mock 对象的 64 位长整数，空实现。
        /// </summary>
        /// <remarks>Sets the long value of this mock; no-op stub.</remarks>
        /// <param name="val">要设置的 64 位长整数（被忽略）/ The long value to set (ignored).</param>
        [Preserve]
        public void SetLong(long val)
        {
        }

        /// <summary>
        /// 设置 mock 对象的字符串，空实现。
        /// </summary>
        /// <remarks>Sets the string value of this mock; no-op stub.</remarks>
        /// <param name="val">要设置的字符串（被忽略）/ The string value to set (ignored).</param>
        [Preserve]
        public void SetString(string val)
        {
        }

        /// <summary>
        /// 将 mock 对象序列化为 JSON 字符串，始终返回空字符串。
        /// </summary>
        /// <remarks>Serializes this mock into a JSON string; always returns an empty string.</remarks>
        /// <returns>始终为空字符串 / Always an empty string.</returns>
        [Preserve]
        public string ToJson()
        {
            return "";
        }

        /// <summary>
        /// 将 mock 对象序列化到指定的 JsonWriter，空实现。
        /// </summary>
        /// <remarks>Serializes this mock into the specified JsonWriter; no-op stub.</remarks>
        /// <param name="writer">接收序列化结果的 JsonWriter（被忽略）/ The JsonWriter that receives the serialization result (ignored).</param>
        [Preserve]
        public void ToJson(JsonWriter writer)
        {
        }


        /// <summary>
        /// 获取 IList 是否具有固定大小，始终返回 true。
        /// </summary>
        /// <remarks>Gets whether the IList has a fixed size; always returns true.</remarks>
        /// <value>始终为 true / Always true.</value>
        bool IList.IsFixedSize
        {
            get { return true; }
        }

        /// <summary>
        /// 获取 IList 是否只读，始终返回 true。
        /// </summary>
        /// <remarks>Gets whether the IList is read-only; always returns true.</remarks>
        /// <value>始终为 true / Always true.</value>
        bool IList.IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// 获取或设置指定索引处的元素，mock 实现。
        /// </summary>
        /// <remarks>Gets or sets the element at the specified index; mock stub.</remarks>
        /// <param name="index">要访问的元素索引 / The zero-based index of the element to access.</param>
        /// <value>获取时始终返回 null；设置时为空操作 / Always returns null on get; no-op on set.</value>
        object IList.this[int index]
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// 向 IList 添加元素，mock 实现始终返回 0。
        /// </summary>
        /// <remarks>Adds an item to the IList; mock stub always returns 0.</remarks>
        /// <param name="value">要添加的对象（被忽略）/ The object to add (ignored).</param>
        /// <returns>始终为 0 / Always 0.</returns>
        int IList.Add(object value)
        {
            return 0;
        }

        /// <summary>
        /// 移除 IList 中的所有元素，空实现。
        /// </summary>
        /// <remarks>Removes all items from the IList; no-op stub.</remarks>
        void IList.Clear()
        {
        }

        /// <summary>
        /// 判断 IList 是否包含指定值，始终返回 false。
        /// </summary>
        /// <remarks>Determines whether the IList contains a specific value; always returns false.</remarks>
        /// <param name="value">要在 IList 中定位的对象 / The object to locate in the IList.</param>
        /// <returns>始终为 false / Always false.</returns>
        bool IList.Contains(object value)
        {
            return false;
        }

        /// <summary>
        /// 确定 IList 中指定元素的索引，始终返回 -1。
        /// </summary>
        /// <remarks>Determines the index of a specific item in the IList; always returns -1.</remarks>
        /// <param name="value">要在 IList 中定位的对象 / The object to locate in the IList.</param>
        /// <returns>始终为 -1 / Always -1.</returns>
        int IList.IndexOf(object value)
        {
            return -1;
        }

        /// <summary>
        /// 在指定索引处插入元素，空实现。
        /// </summary>
        /// <remarks>Inserts an item to the IList at the specified index; no-op stub.</remarks>
        /// <param name="i">从零开始的插入索引 / The zero-based index at which to insert.</param>
        /// <param name="v">要插入的对象（被忽略）/ The object to insert (ignored).</param>
        void IList.Insert(int i, object v)
        {
        }

        /// <summary>
        /// 从 IList 中移除指定对象的第一个匹配项，空实现。
        /// </summary>
        /// <remarks>Removes the first occurrence of a specific object from the IList; no-op stub.</remarks>
        /// <param name="value">要从 IList 中移除的对象 / The object to remove from the IList.</param>
        void IList.Remove(object value)
        {
        }

        /// <summary>
        /// 移除指定索引处的 IList 元素，空实现。
        /// </summary>
        /// <remarks>Removes the IList item at the specified index; no-op stub.</remarks>
        /// <param name="index">从零开始的移除索引 / The zero-based index of the item to remove.</param>
        void IList.RemoveAt(int index)
        {
        }


        /// <summary>
        /// 获取 ICollection 中包含的元素数，始终返回 0。
        /// </summary>
        /// <remarks>Gets the number of elements contained in the ICollection; always returns 0.</remarks>
        /// <value>始终为 0 / Always 0.</value>
        int ICollection.Count
        {
            get { return 0; }
        }

        /// <summary>
        /// 获取对 ICollection 的访问是否同步，始终返回 false。
        /// </summary>
        /// <remarks>Gets a value indicating whether access to the ICollection is synchronized; always returns false.</remarks>
        /// <value>始终为 false / Always false.</value>
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// 获取用于同步对 ICollection 访问的对象，始终返回 null。
        /// </summary>
        /// <remarks>Gets an object that can be used to synchronize access to the ICollection; always returns null.</remarks>
        /// <value>始终为 null / Always null.</value>
        object ICollection.SyncRoot
        {
            get { return null; }
        }

        /// <summary>
        /// 从指定索引开始将 ICollection 元素复制到 Array，空实现。
        /// </summary>
        /// <remarks>Copies the elements of the ICollection to an Array starting at a particular index; no-op stub.</remarks>
        /// <param name="array">作为复制目标的一维 Array / The one-dimensional Array that is the destination of the elements copied.</param>
        /// <param name="index">array 中从零开始的复制起始索引 / The zero-based index in array at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index)
        {
        }


        /// <summary>
        /// 返回循环访问集合的枚举器，始终返回 null。
        /// </summary>
        /// <remarks>Returns an enumerator that iterates through a collection; always returns null.</remarks>
        /// <returns>始终为 null / Always null.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }


        /// <summary>
        /// 获取 IDictionary 是否具有固定大小，始终返回 true。
        /// </summary>
        /// <remarks>Gets whether the IDictionary has a fixed size; always returns true.</remarks>
        /// <value>始终为 true / Always true.</value>
        bool IDictionary.IsFixedSize
        {
            get { return true; }
        }

        /// <summary>
        /// 获取 IDictionary 是否只读，始终返回 true。
        /// </summary>
        /// <remarks>Gets whether the IDictionary is read-only; always returns true.</remarks>
        /// <value>始终为 true / Always true.</value>
        bool IDictionary.IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// 获取包含 IDictionary 键的 ICollection，始终返回 null。
        /// </summary>
        /// <remarks>Gets an ICollection containing the keys of the IDictionary; always returns null.</remarks>
        /// <value>始终为 null / Always null.</value>
        ICollection IDictionary.Keys
        {
            get { return null; }
        }

        /// <summary>
        /// 获取包含 IDictionary 值的 ICollection，始终返回 null。
        /// </summary>
        /// <remarks>Gets an ICollection containing the values of the IDictionary; always returns null.</remarks>
        /// <value>始终为 null / Always null.</value>
        ICollection IDictionary.Values
        {
            get { return null; }
        }

        /// <summary>
        /// 获取或设置具有指定键的元素，mock 实现。
        /// </summary>
        /// <remarks>Gets or sets the element with the specified key; mock stub.</remarks>
        /// <param name="key">要获取或设置其值的键 / The key whose value to get or set.</param>
        /// <value>获取时始终返回 null；设置时为空操作 / Always returns null on get; no-op on set.</value>
        object IDictionary.this[object key]
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// 向 IDictionary 添加带有所提供键和值的元素，空实现。
        /// </summary>
        /// <remarks>Adds an element with the provided key and value to the IDictionary; no-op stub.</remarks>
        /// <param name="k">作为要添加元素的键的对象 / The object to use as the key of the element to add.</param>
        /// <param name="v">作为要添加元素的值的对象 / The object to use as the value of the element to add.</param>
        void IDictionary.Add(object k, object v)
        {
        }

        /// <summary>
        /// 从 IDictionary 中移除所有元素，空实现。
        /// </summary>
        /// <remarks>Removes all elements from the IDictionary; no-op stub.</remarks>
        void IDictionary.Clear()
        {
        }

        /// <summary>
        /// 确定 IDictionary 是否包含具有指定键的元素，始终返回 false。
        /// </summary>
        /// <remarks>Determines whether the IDictionary contains an element with the specified key; always returns false.</remarks>
        /// <param name="key">要在 IDictionary 中定位的键 / The key to locate in the IDictionary.</param>
        /// <returns>始终为 false / Always false.</returns>
        bool IDictionary.Contains(object key)
        {
            return false;
        }

        /// <summary>
        /// 从 IDictionary 中移除带指定键的元素，空实现。
        /// </summary>
        /// <remarks>Removes the element with the specified key from the IDictionary; no-op stub.</remarks>
        /// <param name="key">要移除的元素的键 / The key of the element to remove.</param>
        void IDictionary.Remove(object key)
        {
        }

        /// <summary>
        /// 返回循环访问 IDictionary 的枚举器，始终返回 null。
        /// </summary>
        /// <remarks>Returns an IDictionaryEnumerator for the IDictionary; always returns null.</remarks>
        /// <returns>始终为 null / Always null.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return null;
        }


        /// <summary>
        /// 获取或设置指定索引处的元素，mock 实现。
        /// </summary>
        /// <remarks>Gets or sets the element at the specified index; mock stub.</remarks>
        /// <param name="idx">要访问的元素从零开始的索引 / The zero-based index of the element to access.</param>
        /// <value>获取时始终返回 null；设置时为空操作 / Always returns null on get; no-op on set.</value>
        object IOrderedDictionary.this[int idx]
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// 返回循环访问 IOrderedDictionary 的枚举器，始终返回 null。
        /// </summary>
        /// <remarks>Returns an IDictionaryEnumerator for the IOrderedDictionary; always returns null.</remarks>
        /// <returns>始终为 null / Always null.</returns>
        IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
        {
            return null;
        }

        /// <summary>
        /// 在指定索引处向 IOrderedDictionary 插入带有所提供键和值的元素，空实现。
        /// </summary>
        /// <remarks>Inserts an element with the provided key and value into the IOrderedDictionary at the specified index; no-op stub.</remarks>
        /// <param name="i">从零开始的插入索引 / The zero-based index at which to insert.</param>
        /// <param name="k">作为要插入元素的键的对象 / The object to use as the key of the element to insert.</param>
        /// <param name="v">作为要插入元素的值的对象 / The object to use as the value of the element to insert.</param>
        void IOrderedDictionary.Insert(int i, object k, object v)
        {
        }

        /// <summary>
        /// 移除指定索引处的 IOrderedDictionary 元素，空实现。
        /// </summary>
        /// <remarks>Removes the IOrderedDictionary element at the specified index; no-op stub.</remarks>
        /// <param name="i">从零开始的移除索引 / The zero-based index of the element to remove.</param>
        void IOrderedDictionary.RemoveAt(int i)
        {
        }
    }
}