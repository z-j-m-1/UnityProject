#region Header

/**
 * Lexer.cs
 *   JSON lexer implementation based on a finite state machine.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 表示有限状态机的上下文，在词法分析过程中传递状态信息。
    /// </summary>
    /// <remarks>
    /// Represents the context of the finite state machine, carrying state
    /// information during lexical analysis.
    /// </remarks>
    internal class FsmContext
    {
        /// <summary>
        /// 获取或设置一个值，指示当前状态处理结束后是否应返回词法单元。
        /// </summary>
        /// <remarks>
        /// Gets or sets a value indicating whether a token should be returned
        /// after the current state has been processed.
        /// </remarks>
        public bool Return;

        /// <summary>
        /// 获取或设置有限状态机的下一个状态索引。
        /// </summary>
        /// <remarks>
        /// Gets or sets the next state index of the finite state machine.
        /// </remarks>
        public int NextState;

        /// <summary>
        /// 获取与之关联的词法分析器实例。
        /// </summary>
        /// <remarks>
        /// Gets the associated lexer instance.
        /// </remarks>
        public Lexer L;

        /// <summary>
        /// 获取或设置保存的状态堆栈值，用于转义序列处理后的状态恢复。
        /// </summary>
        /// <remarks>
        /// Gets or sets the saved state stack value used to restore state after
        /// escape sequence processing.
        /// </remarks>
        public int StateStack;
    }


    /// <summary>
    /// 基于有限状态机的 JSON 词法分析器，将 JSON 文本切分为词法单元（Token）流。
    /// </summary>
    /// <remarks>
    /// JSON lexer implementation based on a finite state machine. Splits JSON
    /// text into a stream of tokens.
    /// </remarks>
    internal class Lexer
    {
        #region Fields

        private delegate bool StateHandler(FsmContext ctx);

        private static readonly int[] fsm_return_table;
        private static readonly StateHandler[] fsm_handler_table;

        private bool allow_comments;
        private bool allow_single_quoted_strings;
        private bool end_of_input;
        private FsmContext fsm_context;
        private int input_buffer;
        private int input_char;
        private TextReader reader;
        private int state;
        private StringBuilder string_buffer;
        private string string_value;
        private int token;
        private int unichar;

        #endregion


        #region Properties

        /// <summary>
        /// 获取或设置一个值，指示词法分析器是否允许在 JSON 输入中出现注释。
        /// </summary>
        /// <remarks>
        /// Gets or sets a value indicating whether comments are allowed in the
        /// JSON input.
        /// </remarks>
        /// <value>如果允许注释，则为 <c>true</c>；否则为 <c>false</c>。/ <c>true</c> if comments are allowed; otherwise, <c>false</c>.</value>
        public bool AllowComments
        {
            get { return allow_comments; }
            set { allow_comments = value; }
        }

        /// <summary>
        /// 获取或设置一个值，指示词法分析器是否允许使用单引号包裹的字符串。
        /// </summary>
        /// <remarks>
        /// Gets or sets a value indicating whether single-quoted strings are
        /// allowed.
        /// </remarks>
        /// <value>如果允许单引号字符串，则为 <c>true</c>；否则为 <c>false</c>。/ <c>true</c> if single-quoted strings are allowed; otherwise, <c>false</c>.</value>
        public bool AllowSingleQuotedStrings
        {
            get { return allow_single_quoted_strings; }
            set { allow_single_quoted_strings = value; }
        }

        /// <summary>
        /// 获取一个值，指示是否已到达输入的末尾。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the end of the input has been
        /// reached.
        /// </remarks>
        /// <value>如果已到达输入末尾，则为 <c>true</c>；否则为 <c>false</c>。/ <c>true</c> if the end of the input has been reached; otherwise, <c>false</c>.</value>
        public bool EndOfInput
        {
            get { return end_of_input; }
        }

        /// <summary>
        /// 获取当前词法单元（以整数形式表示）。
        /// </summary>
        /// <remarks>
        /// Gets the current token, represented as an integer.
        /// </remarks>
        /// <value>当前词法单元的整数标识，对应 <see cref="ParserToken" /> 中的值或单字符码。/ The integer identifier of the current token, corresponding to a value in <see cref="ParserToken" /> or a single character code.</value>
        public int Token
        {
            get { return token; }
        }

        /// <summary>
        /// 获取当前词法单元对应的字符串值。
        /// </summary>
        /// <remarks>
        /// Gets the string value associated with the current token.
        /// </remarks>
        /// <value>当前词法单元的字符串内容。/ The string content of the current token.</value>
        public string StringValue
        {
            get { return string_value; }
        }

        #endregion


        #region Constructors

        static Lexer()
        {
            PopulateFsmTables(out fsm_handler_table, out fsm_return_table);
        }

        /// <summary>
        /// 初始化 <see cref="Lexer" /> 类的新实例，从指定的文本读取器读取 JSON 输入。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="Lexer" /> class that
        /// reads JSON input from the specified text reader.
        /// </remarks>
        /// <param name="reader">提供 JSON 输入的文本读取器。 / The text reader that provides the JSON input.</param>
        public Lexer(TextReader reader)
        {
            allow_comments = true;
            allow_single_quoted_strings = true;

            input_buffer = 0;
            string_buffer = new StringBuilder(128);
            state = 1;
            end_of_input = false;
            this.reader = reader;

            fsm_context = new FsmContext();
            fsm_context.L = this;
        }

        #endregion


        #region Static Methods

        private static int HexValue(int digit)
        {
            switch (digit)
            {
                case 'a':
                case 'A':
                    return 10;

                case 'b':
                case 'B':
                    return 11;

                case 'c':
                case 'C':
                    return 12;

                case 'd':
                case 'D':
                    return 13;

                case 'e':
                case 'E':
                    return 14;

                case 'f':
                case 'F':
                    return 15;

                default:
                    return digit - '0';
            }
        }

        private static void PopulateFsmTables(out StateHandler[] fsm_handler_table, out int[] fsm_return_table)
        {
            // See section A.1. of the manual for details of the finite
            // state machine.
            fsm_handler_table = new StateHandler[28]
            {
                State1,
                State2,
                State3,
                State4,
                State5,
                State6,
                State7,
                State8,
                State9,
                State10,
                State11,
                State12,
                State13,
                State14,
                State15,
                State16,
                State17,
                State18,
                State19,
                State20,
                State21,
                State22,
                State23,
                State24,
                State25,
                State26,
                State27,
                State28,
            };

            fsm_return_table = new int[28]
            {
                (int)ParserToken.Char,
                0,
                (int)ParserToken.Number,
                (int)ParserToken.Number,
                0,
                (int)ParserToken.Number,
                0,
                (int)ParserToken.Number,
                0,
                0,
                (int)ParserToken.True,
                0,
                0,
                0,
                (int)ParserToken.False,
                0,
                0,
                (int)ParserToken.Null,
                (int)ParserToken.CharSeq,
                (int)ParserToken.Char,
                0,
                0,
                (int)ParserToken.CharSeq,
                (int)ParserToken.Char,
                0,
                0,
                0,
                0,
            };
        }

        private static char ProcessEscChar(int esc_char)
        {
            switch (esc_char)
            {
                case '"':
                case '\'':
                case '\\':
                case '/':
                    return Convert.ToChar(esc_char);

                case 'n':
                    return '\n';

                case 't':
                    return '\t';

                case 'r':
                    return '\r';

                case 'b':
                    return '\b';

                case 'f':
                    return '\f';

                default:
                    // Unreachable
                    return '?';
            }
        }

        private static bool State1(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char == ' ' ||
                    (ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r'))
                {
                    continue;
                }

                if (ctx.L.input_char >= '1' && ctx.L.input_char <= '9')
                {
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 3;
                    return true;
                }

                switch (ctx.L.input_char)
                {
                    case '"':
                        ctx.NextState = 19;
                        ctx.Return = true;
                        return true;

                    case ',':
                    case ':':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                        ctx.NextState = 1;
                        ctx.Return = true;
                        return true;

                    case '-':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 2;
                        return true;

                    case '0':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 4;
                        return true;

                    case 'f':
                        ctx.NextState = 12;
                        return true;

                    case 'n':
                        ctx.NextState = 16;
                        return true;

                    case 't':
                        ctx.NextState = 9;
                        return true;

                    case '\'':
                        if (!ctx.L.allow_single_quoted_strings)
                        {
                            return false;
                        }

                        ctx.L.input_char = '"';
                        ctx.NextState = 23;
                        ctx.Return = true;
                        return true;

                    case '/':
                        if (!ctx.L.allow_comments)
                        {
                            return false;
                        }

                        ctx.NextState = 25;
                        return true;

                    default:
                        return false;
                }
            }

            return true;
        }

        private static bool State2(FsmContext ctx)
        {
            ctx.L.GetChar();

            if (ctx.L.input_char >= '1' && ctx.L.input_char <= '9')
            {
                ctx.L.string_buffer.Append((char)ctx.L.input_char);
                ctx.NextState = 3;
                return true;
            }

            switch (ctx.L.input_char)
            {
                case '0':
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 4;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State3(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
                {
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    continue;
                }

                if (ctx.L.input_char == ' ' ||
                    (ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r'))
                {
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }

                switch (ctx.L.input_char)
                {
                    case ',':
                    case ']':
                    case '}':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 1;
                        return true;

                    case '.':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 5;
                        return true;

                    case 'e':
                    case 'E':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 7;
                        return true;

                    default:
                        return false;
                }
            }

            return true;
        }

        private static bool State4(FsmContext ctx)
        {
            ctx.L.GetChar();

            if (ctx.L.input_char == ' ' ||
                (ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r'))
            {
                ctx.Return = true;
                ctx.NextState = 1;
                return true;
            }

            switch (ctx.L.input_char)
            {
                case ',':
                case ']':
                case '}':
                    ctx.L.UngetChar();
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;

                case '.':
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 5;
                    return true;

                case 'e':
                case 'E':
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 7;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State5(FsmContext ctx)
        {
            ctx.L.GetChar();

            if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
            {
                ctx.L.string_buffer.Append((char)ctx.L.input_char);
                ctx.NextState = 6;
                return true;
            }

            return false;
        }

        private static bool State6(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
                {
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    continue;
                }

                if (ctx.L.input_char == ' ' ||
                    (ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r'))
                {
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }

                switch (ctx.L.input_char)
                {
                    case ',':
                    case ']':
                    case '}':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 1;
                        return true;

                    case 'e':
                    case 'E':
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        ctx.NextState = 7;
                        return true;

                    default:
                        return false;
                }
            }

            return true;
        }

        private static bool State7(FsmContext ctx)
        {
            ctx.L.GetChar();

            if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
            {
                ctx.L.string_buffer.Append((char)ctx.L.input_char);
                ctx.NextState = 8;
                return true;
            }

            switch (ctx.L.input_char)
            {
                case '+':
                case '-':
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    ctx.NextState = 8;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State8(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char >= '0' && ctx.L.input_char <= '9')
                {
                    ctx.L.string_buffer.Append((char)ctx.L.input_char);
                    continue;
                }

                if (ctx.L.input_char == ' ' ||
                    (ctx.L.input_char >= '\t' && ctx.L.input_char <= '\r'))
                {
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;
                }

                switch (ctx.L.input_char)
                {
                    case ',':
                    case ']':
                    case '}':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 1;
                        return true;

                    default:
                        return false;
                }
            }

            return true;
        }

        private static bool State9(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'r':
                    ctx.NextState = 10;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State10(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'u':
                    ctx.NextState = 11;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State11(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'e':
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State12(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'a':
                    ctx.NextState = 13;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State13(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'l':
                    ctx.NextState = 14;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State14(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 's':
                    ctx.NextState = 15;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State15(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'e':
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State16(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'u':
                    ctx.NextState = 17;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State17(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'l':
                    ctx.NextState = 18;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State18(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'l':
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State19(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                switch (ctx.L.input_char)
                {
                    case '"':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 20;
                        return true;

                    case '\\':
                        ctx.StateStack = 19;
                        ctx.NextState = 21;
                        return true;

                    default:
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        continue;
                }
            }

            return true;
        }

        private static bool State20(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case '"':
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State21(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case 'u':
                    ctx.NextState = 22;
                    return true;

                case '"':
                case '\'':
                case '/':
                case '\\':
                case 'b':
                case 'f':
                case 'n':
                case 'r':
                case 't':
                    ctx.L.string_buffer.Append(
                        ProcessEscChar(ctx.L.input_char));
                    ctx.NextState = ctx.StateStack;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State22(FsmContext ctx)
        {
            var counter = 0;
            var mult = 4096;

            ctx.L.unichar = 0;

            while (ctx.L.GetChar())
            {
                if ((ctx.L.input_char >= '0' && ctx.L.input_char <= '9') ||
                    (ctx.L.input_char >= 'A' && ctx.L.input_char <= 'F') ||
                    (ctx.L.input_char >= 'a' && ctx.L.input_char <= 'f'))
                {
                    ctx.L.unichar += HexValue(ctx.L.input_char) * mult;

                    counter++;
                    mult /= 16;

                    if (counter == 4)
                    {
                        ctx.L.string_buffer.Append(
                            Convert.ToChar(ctx.L.unichar));
                        ctx.NextState = ctx.StateStack;
                        return true;
                    }

                    continue;
                }

                return false;
            }

            return true;
        }

        private static bool State23(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                switch (ctx.L.input_char)
                {
                    case '\'':
                        ctx.L.UngetChar();
                        ctx.Return = true;
                        ctx.NextState = 24;
                        return true;

                    case '\\':
                        ctx.StateStack = 23;
                        ctx.NextState = 21;
                        return true;

                    default:
                        ctx.L.string_buffer.Append((char)ctx.L.input_char);
                        continue;
                }
            }

            return true;
        }

        private static bool State24(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case '\'':
                    ctx.L.input_char = '"';
                    ctx.Return = true;
                    ctx.NextState = 1;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State25(FsmContext ctx)
        {
            ctx.L.GetChar();

            switch (ctx.L.input_char)
            {
                case '*':
                    ctx.NextState = 27;
                    return true;

                case '/':
                    ctx.NextState = 26;
                    return true;

                default:
                    return false;
            }
        }

        private static bool State26(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char == '\n')
                {
                    ctx.NextState = 1;
                    return true;
                }
            }

            return true;
        }

        private static bool State27(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char == '*')
                {
                    ctx.NextState = 28;
                    return true;
                }
            }

            return true;
        }

        private static bool State28(FsmContext ctx)
        {
            while (ctx.L.GetChar())
            {
                if (ctx.L.input_char == '*')
                {
                    continue;
                }

                if (ctx.L.input_char == '/')
                {
                    ctx.NextState = 1;
                    return true;
                }

                ctx.NextState = 27;
                return true;
            }

            return true;
        }

        #endregion


        private bool GetChar()
        {
            if ((input_char = NextChar()) != -1)
            {
                return true;
            }

            end_of_input = true;
            return false;
        }

        private int NextChar()
        {
            if (input_buffer != 0)
            {
                var tmp = input_buffer;
                input_buffer = 0;

                return tmp;
            }

            return reader.Read();
        }

        /// <summary>
        /// 使用有限状态机读取下一个词法单元。
        /// </summary>
        /// <remarks>
        /// Reads the next token from the input using the finite state machine.
        /// </remarks>
        /// <returns>如果成功读取下一个词法单元，则为 <c>true</c>；如果到达输入末尾，则为 <c>false</c>。 / <c>true</c> if the next token was read successfully; <c>false</c> if the end of the input was reached.</returns>
        /// <exception cref="JsonException">输入包含不符合 JSON 语法的字符或序列。 / The input contains a character or sequence that is not valid JSON syntax.</exception>
        public bool NextToken()
        {
            StateHandler handler;
            fsm_context.Return = false;

            while (true)
            {
                handler = fsm_handler_table[state - 1];

                if (!handler(fsm_context))
                {
                    throw new JsonException(input_char);
                }

                if (end_of_input)
                {
                    return false;
                }

                if (fsm_context.Return)
                {
                    string_value = string_buffer.ToString();
                    string_buffer.Remove(0, string_buffer.Length);
                    token = fsm_return_table[state - 1];

                    if (token == (int)ParserToken.Char)
                    {
                        token = input_char;
                    }

                    state = fsm_context.NextState;

                    return true;
                }

                state = fsm_context.NextState;
            }
        }

        private void UngetChar()
        {
            input_buffer = input_char;
        }
    }
}