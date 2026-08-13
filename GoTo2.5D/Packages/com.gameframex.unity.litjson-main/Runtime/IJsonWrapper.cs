#region Header

/**
 * IJsonWrapper.cs
 *   Interface that represents a type capable of handling all kinds of JSON
 *   data. This is mainly used when mapping objects through JsonMapper, and
 *   it's implemented by JsonData.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


using System.Collections;
using System.Collections.Specialized;


namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// JSON 数据类型枚举，用于标识 JsonData 当前持有的数据类型。
    /// </summary>
    /// <remarks>
    /// Enumeration of JSON data types, used to identify the type of data
    /// currently held by a JsonData instance.
    /// </remarks>
    public enum JsonType
    {
        /// <summary>
        /// 未初始化。
        /// </summary>
        /// <remarks>Not initialized.</remarks>
        None,

        /// <summary>
        /// 对象类型。
        /// </summary>
        /// <remarks>Object type.</remarks>
        Object,

        /// <summary>
        /// 数组类型。
        /// </summary>
        /// <remarks>Array type.</remarks>
        Array,

        /// <summary>
        /// 字符串类型。
        /// </summary>
        /// <remarks>String type.</remarks>
        String,

        /// <summary>
        /// 32 位整数。
        /// </summary>
        /// <remarks>32-bit integer.</remarks>
        Int,

        /// <summary>
        /// 64 位长整数。
        /// </summary>
        /// <remarks>64-bit long integer.</remarks>
        Long,

        /// <summary>
        /// 双精度浮点数。
        /// </summary>
        /// <remarks>Double-precision floating point.</remarks>
        Double,

        /// <summary>
        /// 布尔值。
        /// </summary>
        /// <remarks>Boolean value.</remarks>
        Boolean,
    }

    /// <summary>
    /// 表示能处理各种 JSON 数据的类型的接口，主要由 JsonData 实现，
    /// 用于在 JsonMapper 中作为对象映射的目标类型。
    /// </summary>
    /// <remarks>
    /// Interface that represents a type capable of handling all kinds of JSON
    /// data. This is mainly used when mapping objects through JsonMapper, and
    /// it's implemented by JsonData.
    /// </remarks>
    public interface IJsonWrapper : IList, IOrderedDictionary
    {
        /// <summary>
        /// 获取当前实例是否为数组。
        /// </summary>
        /// <remarks>Gets whether this instance holds an array.</remarks>
        /// <value>如果为数组则为 true；否则为 false / true if this instance holds an array; otherwise, false.</value>
        bool IsArray { get; }

        /// <summary>
        /// 获取当前实例是否为布尔值。
        /// </summary>
        /// <remarks>Gets whether this instance holds a boolean.</remarks>
        /// <value>如果为布尔值则为 true；否则为 false / true if this instance holds a boolean; otherwise, false.</value>
        bool IsBoolean { get; }

        /// <summary>
        /// 获取当前实例是否为双精度浮点数。
        /// </summary>
        /// <remarks>Gets whether this instance holds a double.</remarks>
        /// <value>如果为双精度浮点数则为 true；否则为 false / true if this instance holds a double; otherwise, false.</value>
        bool IsDouble { get; }

        /// <summary>
        /// 获取当前实例是否为 32 位整数。
        /// </summary>
        /// <remarks>Gets whether this instance holds an int.</remarks>
        /// <value>如果为 32 位整数则为 true；否则为 false / true if this instance holds an int; otherwise, false.</value>
        bool IsInt { get; }

        /// <summary>
        /// 获取当前实例是否为 64 位长整数。
        /// </summary>
        /// <remarks>Gets whether this instance holds a long.</remarks>
        /// <value>如果为 64 位长整数则为 true；否则为 false / true if this instance holds a long; otherwise, false.</value>
        bool IsLong { get; }

        /// <summary>
        /// 获取当前实例是否为对象。
        /// </summary>
        /// <remarks>Gets whether this instance holds an object.</remarks>
        /// <value>如果为对象则为 true；否则为 false / true if this instance holds an object; otherwise, false.</value>
        bool IsObject { get; }

        /// <summary>
        /// 获取当前实例是否为字符串。
        /// </summary>
        /// <remarks>Gets whether this instance holds a string.</remarks>
        /// <value>如果为字符串则为 true；否则为 false / true if this instance holds a string; otherwise, false.</value>
        bool IsString { get; }

        /// <summary>
        /// 获取当前实例的布尔值。
        /// </summary>
        /// <remarks>Gets the boolean value held by this instance.</remarks>
        /// <returns>当前实例的布尔值 / The boolean value of this instance.</returns>
        bool GetBoolean();

        /// <summary>
        /// 获取当前实例的双精度浮点数。
        /// </summary>
        /// <remarks>Gets the double value held by this instance.</remarks>
        /// <returns>当前实例的双精度浮点数 / The double value of this instance.</returns>
        double GetDouble();

        /// <summary>
        /// 获取当前实例的 32 位整数。
        /// </summary>
        /// <remarks>Gets the int value held by this instance.</remarks>
        /// <returns>当前实例的 32 位整数 / The int value of this instance.</returns>
        int GetInt();

        /// <summary>
        /// 获取当前实例的 JSON 数据类型。
        /// </summary>
        /// <remarks>Gets the JSON data type of this instance.</remarks>
        /// <returns>当前实例的 JSON 数据类型 / The JSON data type of this instance.</returns>
        JsonType GetJsonType();

        /// <summary>
        /// 获取当前实例的 64 位长整数。
        /// </summary>
        /// <remarks>Gets the long value held by this instance.</remarks>
        /// <returns>当前实例的 64 位长整数 / The long value of this instance.</returns>
        long GetLong();

        /// <summary>
        /// 获取当前实例的字符串。
        /// </summary>
        /// <remarks>Gets the string value held by this instance.</remarks>
        /// <returns>当前实例的字符串 / The string value of this instance.</returns>
        string GetString();

        /// <summary>
        /// 设置当前实例的布尔值。
        /// </summary>
        /// <remarks>Sets the boolean value of this instance.</remarks>
        /// <param name="val">要设置的布尔值 / The boolean value to set.</param>
        void SetBoolean(bool val);

        /// <summary>
        /// 设置当前实例的双精度浮点数。
        /// </summary>
        /// <remarks>Sets the double value of this instance.</remarks>
        /// <param name="val">要设置的双精度浮点数 / The double value to set.</param>
        void SetDouble(double val);

        /// <summary>
        /// 设置当前实例的 32 位整数。
        /// </summary>
        /// <remarks>Sets the int value of this instance.</remarks>
        /// <param name="val">要设置的 32 位整数 / The int value to set.</param>
        void SetInt(int val);

        /// <summary>
        /// 设置当前实例的 JSON 数据类型。
        /// </summary>
        /// <remarks>Sets the JSON data type of this instance.</remarks>
        /// <param name="type">要设置的 JSON 数据类型 / The JSON data type to set.</param>
        void SetJsonType(JsonType type);

        /// <summary>
        /// 设置当前实例的 64 位长整数。
        /// </summary>
        /// <remarks>Sets the long value of this instance.</remarks>
        /// <param name="val">要设置的 64 位长整数 / The long value to set.</param>
        void SetLong(long val);

        /// <summary>
        /// 设置当前实例的字符串。
        /// </summary>
        /// <remarks>Sets the string value of this instance.</remarks>
        /// <param name="val">要设置的字符串 / The string value to set.</param>
        void SetString(string val);

        /// <summary>
        /// 将当前实例序列化为 JSON 字符串。
        /// </summary>
        /// <remarks>Serializes this instance into a JSON string.</remarks>
        /// <returns>序列化后的 JSON 字符串 / The serialized JSON string.</returns>
        string ToJson();

        /// <summary>
        /// 将当前实例序列化到指定的 JsonWriter。
        /// </summary>
        /// <remarks>Serializes this instance into the specified JsonWriter.</remarks>
        /// <param name="writer">接收序列化结果的 JsonWriter / The JsonWriter that receives the serialization result.</param>
        void ToJson(JsonWriter writer);
    }
}