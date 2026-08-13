#region Header

/**
 * ParserToken.cs
 *   Internal representation of the tokens used by the lexer and the parser.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 表示词法分析器和解析器使用的 Token 类型。
    /// </summary>
    /// <remarks>
    /// Internal representation of the tokens used by the lexer and the parser.
    /// </remarks>
    internal enum ParserToken
    {
        // Lexer tokens (see section A.1.1. of the manual)
        /// <summary>
        /// 空词法单元（初始状态或无 Token）。
        /// </summary>
        /// <remarks>
        /// No token (initial state or absence of a token).
        /// </remarks>
        None = char.MaxValue + 1,

        /// <summary>
        /// 数字字面量。
        /// </summary>
        /// <remarks>
        /// Numeric literal.
        /// </remarks>
        Number,

        /// <summary>
        /// 布尔字面量 <c>true</c>。
        /// </summary>
        /// <remarks>
        /// Boolean literal <c>true</c>.
        /// </remarks>
        True,

        /// <summary>
        /// 布尔字面量 <c>false</c>。
        /// </summary>
        /// <remarks>
        /// Boolean literal <c>false</c>.
        /// </remarks>
        False,

        /// <summary>
        /// 空字面量 <c>null</c>。
        /// </summary>
        /// <remarks>
        /// Null literal <c>null</c>.
        /// </remarks>
        Null,

        /// <summary>
        /// 字符串的一部分（已解析的字符序列）。
        /// </summary>
        /// <remarks>
        /// A portion of a string (parsed character sequence).
        /// </remarks>
        CharSeq,

        // Single char
        /// <summary>
        /// 单字符 Token（如 <c>{</c>、<c>}</c>、<c>[</c>、<c>]</c>、<c>,</c>、<c>:</c>）。
        /// </summary>
        /// <remarks>
        /// Single character token (e.g. <c>{</c>, <c>}</c>, <c>[</c>, <c>]</c>, <c>,</c>, <c>:</c>).
        /// </remarks>
        Char,

        // Parser Rules (see section A.2.1 of the manual)
        /// <summary>
        /// 文本（字符串内容）解析规则。
        /// </summary>
        /// <remarks>
        /// Text (string content) parser rule.
        /// </remarks>
        Text,

        /// <summary>
        /// 对象解析规则起始。
        /// </summary>
        /// <remarks>
        /// Object parser rule start.
        /// </remarks>
        Object,

        /// <summary>
        /// 对象解析规则的后续状态。
        /// </summary>
        /// <remarks>
        /// Object parser rule prime (continuation) state.
        /// </remarks>
        ObjectPrime,

        /// <summary>
        /// 键值对（成员）解析规则。
        /// </summary>
        /// <remarks>
        /// Pair (member) parser rule.
        /// </remarks>
        Pair,

        /// <summary>
        /// 键值对后续（多个成员）解析规则。
        /// </summary>
        /// <remarks>
        /// Pair rest (subsequent members) parser rule.
        /// </remarks>
        PairRest,

        /// <summary>
        /// 数组解析规则起始。
        /// </summary>
        /// <remarks>
        /// Array parser rule start.
        /// </remarks>
        Array,

        /// <summary>
        /// 数组解析规则的后续状态。
        /// </summary>
        /// <remarks>
        /// Array parser rule prime (continuation) state.
        /// </remarks>
        ArrayPrime,

        /// <summary>
        /// 值解析规则起始。
        /// </summary>
        /// <remarks>
        /// Value parser rule start.
        /// </remarks>
        Value,

        /// <summary>
        /// 值后续（多个值）解析规则。
        /// </summary>
        /// <remarks>
        /// Value rest (subsequent values) parser rule.
        /// </remarks>
        ValueRest,

        /// <summary>
        /// 字符串解析规则。
        /// </summary>
        /// <remarks>
        /// String parser rule.
        /// </remarks>
        String,

        // End of input
        /// <summary>
        /// 输入结束标记。
        /// </summary>
        /// <remarks>
        /// End of input marker.
        /// </remarks>
        End,

        // The empty rule
        /// <summary>
        /// 空规则（epsilon 产生式）。
        /// </summary>
        /// <remarks>
        /// The empty rule (epsilon production).
        /// </remarks>
        Epsilon,
    }
}