#region Header

/**
 * JsonException.cs
 *   Base class throwed by LitJSON when a parsing error occurs.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


using System;
using UnityEngine.Scripting;


namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// JSON 解析错误时抛出的异常，是 LitJSON 中所有解析异常的基类。
    /// </summary>
    /// <remarks>
    /// Base class thrown by LitJSON when a parsing error occurs.
    /// </remarks>
    public class JsonException :
#if NETSTANDARD1_5
        Exception
#else
        ApplicationException
#endif
    {
        /// <summary>
        /// 初始化 <see cref="JsonException"/> 类的新实例，使用默认错误消息。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonException"/> class with a default error message.
        /// </remarks>
        [Preserve]
        public JsonException() : base()
        {
        }

        /// <summary>
        /// 使用无效的解析器词牌初始化 <see cref="JsonException"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonException"/> class with an invalid parser token.
        /// </remarks>
        /// <param name="token">输入中遇到的无效词牌 / The invalid parser token encountered in the input</param>
        internal JsonException(ParserToken token) :
            base(string.Format(
                     "Invalid token '{0}' in input string", token))
        {
        }

        /// <summary>
        /// 使用无效的解析器词牌和导致此异常的内部异常初始化 <see cref="JsonException"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonException"/> class with an invalid parser token and the inner exception that caused this exception.
        /// </remarks>
        /// <param name="token">输入中遇到的无效词牌 / The invalid parser token encountered in the input</param>
        /// <param name="inner_exception">导致当前异常的内部异常引用 / The exception that is the cause of the current exception</param>
        internal JsonException(ParserToken token,
            Exception inner_exception) :
            base(string.Format(
                     "Invalid token '{0}' in input string", token),
                 inner_exception)
        {
        }

        /// <summary>
        /// 使用无效的字符初始化 <see cref="JsonException"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonException"/> class with an invalid character.
        /// </remarks>
        /// <param name="c">输入中遇到的无效字符的码点 / The code point of the invalid character encountered in the input</param>
        internal JsonException(int c) :
            base(string.Format(
                     "Invalid character '{0}' in input string", (char)c))
        {
        }

        /// <summary>
        /// 使用无效的字符和导致此异常的内部异常初始化 <see cref="JsonException"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonException"/> class with an invalid character and the inner exception that caused this exception.
        /// </remarks>
        /// <param name="c">输入中遇到的无效字符的码点 / The code point of the invalid character encountered in the input</param>
        /// <param name="inner_exception">导致当前异常的内部异常引用 / The exception that is the cause of the current exception</param>
        internal JsonException(int c, Exception inner_exception) :
            base(string.Format(
                     "Invalid character '{0}' in input string", (char)c),
                 inner_exception)
        {
        }


        /// <summary>
        /// 使用指定的错误消息初始化 <see cref="JsonException"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonException"/> class with a specified error message.
        /// </remarks>
        /// <param name="message">描述错误的消息 / The message that describes the error</param>
        [Preserve]
        public JsonException(string message) : base(message)
        {
        }

        /// <summary>
        /// 使用指定的错误消息和对导致此异常的内部异常的引用初始化 <see cref="JsonException"/> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </remarks>
        /// <param name="message">描述错误的消息 / The error message that explains the reason for the exception</param>
        /// <param name="inner_exception">导致当前异常的内部异常引用 / The exception that is the cause of the current exception</param>
        [Preserve]
        public JsonException(string message, Exception inner_exception) :
            base(message, inner_exception)
        {
        }
    }
}