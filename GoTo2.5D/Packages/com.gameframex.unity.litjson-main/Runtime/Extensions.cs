using UnityEngine;
using UnityEngine.Scripting;
using System.Collections;

namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 提供 JsonWriter 的属性写入扩展方法。
    /// </summary>
    /// <remarks>
    /// Provides extension methods for writing properties to JsonWriter.
    /// </remarks>
    public static class Extensions
    {
        /// <summary>
        /// 写入指定名称和 long 值的 JSON 属性。
        /// </summary>
        /// <remarks>
        /// Writes a JSON property with the specified name and long value.
        /// </remarks>
        /// <param name="w">目标 JsonWriter / Target JsonWriter</param>
        /// <param name="name">属性名 / Property name</param>
        /// <param name="value">属性值 / Property value</param>
        [Preserve]
        public static void WriteProperty(this JsonWriter w, string name, long value)
        {
            w.WritePropertyName(name);
            w.Write(value);
        }

        /// <summary>
        /// 写入指定名称和 string 值的 JSON 属性。
        /// </summary>
        /// <remarks>
        /// Writes a JSON property with the specified name and string value.
        /// </remarks>
        /// <param name="w">目标 JsonWriter / Target JsonWriter</param>
        /// <param name="name">属性名 / Property name</param>
        /// <param name="value">属性值 / Property value</param>
        [Preserve]
        public static void WriteProperty(this JsonWriter w, string name, string value)
        {
            w.WritePropertyName(name);
            w.Write(value);
        }

        /// <summary>
        /// 写入指定名称和 bool 值的 JSON 属性。
        /// </summary>
        /// <remarks>
        /// Writes a JSON property with the specified name and bool value.
        /// </remarks>
        /// <param name="w">目标 JsonWriter / Target JsonWriter</param>
        /// <param name="name">属性名 / Property name</param>
        /// <param name="value">属性值 / Property value</param>
        [Preserve]
        public static void WriteProperty(this JsonWriter w, string name, bool value)
        {
            w.WritePropertyName(name);
            w.Write(value);
        }

        /// <summary>
        /// 写入指定名称和 double 值的 JSON 属性。
        /// </summary>
        /// <remarks>
        /// Writes a JSON property with the specified name and double value.
        /// </remarks>
        /// <param name="w">目标 JsonWriter / Target JsonWriter</param>
        /// <param name="name">属性名 / Property name</param>
        /// <param name="value">属性值 / Property value</param>
        [Preserve]
        public static void WriteProperty(this JsonWriter w, string name, double value)
        {
            w.WritePropertyName(name);
            w.Write(value);
        }
    }
}