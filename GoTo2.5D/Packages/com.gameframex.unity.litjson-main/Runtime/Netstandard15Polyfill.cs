#if NETSTANDARD1_5
using System;
using System.Reflection;
namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 为 .NET Standard 1.5 提供 <see cref="Type"/> 上缺失 API 的 polyfill 扩展方法。
    /// </summary>
    /// <remarks>
    /// Provides polyfill extension methods for the missing <see cref="Type"/> APIs under .NET Standard 1.5,
    /// routing them through <c>TypeInfo</c> where the actual members live in that target framework.
    /// </summary>
    internal static class Netstandard15Polyfill
    {
        /// <summary>
        /// 搜索具有指定名称的接口，仅在 .NET Standard 1.5 上使用。
        /// </summary>
        /// <remarks>
        /// Searches for the interface with the specified name. This polyfill is only compiled under .NET Standard 1.5
        /// and delegates to <c>TypeInfo.GetInterface</c> because the direct member is unavailable on that target.
        /// </remarks>
        /// <param name="type">要在其上查找接口的类型 / The type on which to look up the interface</param>
        /// <param name="name">要查找的接口的完全限定名或部分名 / The fully or partially qualified name of the interface to find</param>
        /// <returns>表示接口的 <see cref="Type"/> 对象；若未找到则为 null / A <see cref="Type"/> object representing the interface; null if not found</returns>
        internal static Type GetInterface(this Type type, string name)
        {
            return type.GetTypeInfo().GetInterface(name);
        }

        /// <summary>
        /// 判断当前类型是否为类或委托，仅在 .NET Standard 1.5 上使用。
        /// </summary>
        /// <remarks>
        /// Determines whether the current type is a class or a delegate. This polyfill is only compiled under .NET Standard 1.5
        /// and delegates to <c>TypeInfo.IsClass</c> because the direct member is unavailable on that target.
        /// </remarks>
        /// <param name="type">要判断的类型 / The type to test</param>
        /// <returns>若当前类型是类或委托则为 true，否则为 false / true if the current type is a class or delegate; otherwise, false</returns>
        internal static bool IsClass(this Type type)
        {
            return type.GetTypeInfo().IsClass;
        }

        /// <summary>
        /// 判断当前类型是否为枚举，仅在 .NET Standard 1.5 上使用。
        /// </summary>
        /// <remarks>
        /// Determines whether the current type is an enumeration. This polyfill is only compiled under .NET Standard 1.5
        /// and delegates to <c>TypeInfo.IsEnum</c> because the direct member is unavailable on that target.
        /// </remarks>
        /// <param name="type">要判断的类型 / The type to test</param>
        /// <returns>若当前类型是枚举则为 true，否则为 false / true if the current type is an enumeration; otherwise, false</returns>
        internal static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }
    }
}
#endif