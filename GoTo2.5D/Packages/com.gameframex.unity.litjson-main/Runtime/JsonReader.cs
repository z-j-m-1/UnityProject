#region Header

/**
 * JsonReader.cs
 *   Stream-like access to JSON text.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine.Scripting;


namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 表示 JSON 文本解析过程中产生的词法单元类型。
    /// </summary>
    /// <remarks>
    /// Indicates the kind of token produced while parsing JSON text.
    /// </remarks>
    public enum JsonToken
    {
        /// <summary>无 token。/ No token.</summary>
        None,

        /// <summary>对象开始（'{'）。/ Object start ('{' character).</summary>
        ObjectStart,

        /// <summary>属性名。/ Property name within an object.</summary>
        PropertyName,

        /// <summary>对象结束（'}'）。/ Object end ('}' character).</summary>
        ObjectEnd,

        /// <summary>数组开始（'['）。/ Array start ('[' character).</summary>
        ArrayStart,

        /// <summary>数组结束（']'）。/ Array end (']' character).</summary>
        ArrayEnd,

        /// <summary>32 位整数。/ 32-bit integer value.</summary>
        Int,

        /// <summary>64 位长整数。/ 64-bit long integer value.</summary>
        Long,

        /// <summary>双精度浮点数。/ Double-precision floating-point value.</summary>
        Double,

        /// <summary>字符串。/ String value.</summary>
        String,

        /// <summary>布尔值。/ Boolean value.</summary>
        Boolean,

        /// <summary>null 值。/ Null value.</summary>
        Null,
    }


    /// <summary>
    /// 提供对 JSON 文本的流式只读访问，按词法单元（Token）逐个推进。
    /// </summary>
    /// <remarks>
    /// Provides stream-like, forward-only access to JSON text, advancing one token at a time.
    /// </remarks>
    public class JsonReader
    {
        #region Fields

        private static readonly IDictionary<int, IDictionary<int, int[]>> parse_table;

        private Stack<int> automaton_stack;
        private int current_input;
        private int current_symbol;
        private bool end_of_json;
        private bool end_of_input;
        private Lexer lexer;
        private bool parser_in_string;
        private bool parser_return;
        private bool read_started;
        private TextReader reader;
        private bool reader_is_owned;
        private bool skip_non_members;
        private object token_value;
        private JsonToken token;

        #endregion


        #region Public Properties

        /// <summary>
        /// 获取或设置是否允许 JSON 文本中出现注释。
        /// </summary>
        /// <remarks>
        /// Gets or sets whether comments are allowed in the JSON text.
        /// </remarks>
        [Preserve]
        public bool AllowComments
        {
            get { return lexer.AllowComments; }
            set { lexer.AllowComments = value; }
        }

        /// <summary>
        /// 获取或设置是否允许使用单引号包裹的字符串。
        /// </summary>
        /// <remarks>
        /// Gets or sets whether single-quoted strings are allowed.
        /// </remarks>
        [Preserve]
        public bool AllowSingleQuotedStrings
        {
            get { return lexer.AllowSingleQuotedStrings; }
            set { lexer.AllowSingleQuotedStrings = value; }
        }

        /// <summary>
        /// 获取或设置反序列化时是否跳过目标对象中不存在的成员。
        /// </summary>
        /// <remarks>
        /// Gets or sets whether non-member properties are skipped during deserialization.
        /// </remarks>
        [Preserve]
        public bool SkipNonMembers
        {
            get { return skip_non_members; }
            set { skip_non_members = value; }
        }

        /// <summary>
        /// 获取是否已到达输入流的末尾。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the end of the input has been reached.
        /// </remarks>
        [Preserve]
        public bool EndOfInput
        {
            get { return end_of_input; }
        }

        /// <summary>
        /// 获取是否已到达当前 JSON 文本的末尾。
        /// </summary>
        /// <remarks>
        /// Gets a value indicating whether the end of the current JSON text has been reached.
        /// </remarks>
        [Preserve]
        public bool EndOfJson
        {
            get { return end_of_json; }
        }

        /// <summary>
        /// 获取最近一次读取到的词法单元类型。
        /// </summary>
        /// <remarks>
        /// Gets the type of the token most recently read.
        /// </remarks>
        [Preserve]
        public JsonToken Token
        {
            get { return token; }
        }

        /// <summary>
        /// 获取最近一次读取到的词法单元的值。
        /// </summary>
        /// <remarks>
        /// Gets the value of the token most recently read.
        /// </remarks>
        [Preserve]
        public object Value
        {
            get { return token_value; }
        }

        #endregion


        #region Constructors

        static JsonReader()
        {
            parse_table = PopulateParseTable();
        }

        /// <summary>
        /// 使用指定的 JSON 字符串初始化 <see cref="JsonReader" /> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonReader" /> class using the provided JSON string.
        /// </remarks>
        /// <param name="json_text">要读取的 JSON 字符串。/ The JSON string to read.</param>
        [Preserve]
        public JsonReader(string json_text) :
            this(new StringReader(json_text), true)
        {
        }

        /// <summary>
        /// 使用指定的 <see cref="TextReader" /> 初始化 <see cref="JsonReader" /> 类的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonReader" /> class using the provided <see cref="TextReader" />.
        /// </remarks>
        /// <param name="reader">提供 JSON 文本的文本读取器。/ The text reader that supplies the JSON text.</param>
        [Preserve]
        public JsonReader(TextReader reader) :
            this(reader, false)
        {
        }

        private JsonReader(TextReader reader, bool owned)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            parser_in_string = false;
            parser_return = false;

            read_started = false;
            automaton_stack = new Stack<int>();
            automaton_stack.Push((int)ParserToken.End);
            automaton_stack.Push((int)ParserToken.Text);

            lexer = new Lexer(reader);

            end_of_input = false;
            end_of_json = false;

            skip_non_members = true;

            this.reader = reader;
            reader_is_owned = owned;
        }

        #endregion


        #region Static Methods

        private static IDictionary<int, IDictionary<int, int[]>> PopulateParseTable()
        {
            // See section A.2. of the manual for details
            IDictionary<int, IDictionary<int, int[]>> parse_table = new Dictionary<int, IDictionary<int, int[]>>();

            TableAddRow(parse_table, ParserToken.Array);
            TableAddCol(parse_table, ParserToken.Array, '[',
                        '[',
                        (int)ParserToken.ArrayPrime);

            TableAddRow(parse_table, ParserToken.ArrayPrime);
            TableAddCol(parse_table, ParserToken.ArrayPrime, '"',
                        (int)ParserToken.Value,
                        (int)ParserToken.ValueRest,
                        ']');
            TableAddCol(parse_table, ParserToken.ArrayPrime, '[',
                        (int)ParserToken.Value,
                        (int)ParserToken.ValueRest,
                        ']');
            TableAddCol(parse_table, ParserToken.ArrayPrime, ']',
                        ']');
            TableAddCol(parse_table, ParserToken.ArrayPrime, '{',
                        (int)ParserToken.Value,
                        (int)ParserToken.ValueRest,
                        ']');
            TableAddCol(parse_table, ParserToken.ArrayPrime, (int)ParserToken.Number,
                        (int)ParserToken.Value,
                        (int)ParserToken.ValueRest,
                        ']');
            TableAddCol(parse_table, ParserToken.ArrayPrime, (int)ParserToken.True,
                        (int)ParserToken.Value,
                        (int)ParserToken.ValueRest,
                        ']');
            TableAddCol(parse_table, ParserToken.ArrayPrime, (int)ParserToken.False,
                        (int)ParserToken.Value,
                        (int)ParserToken.ValueRest,
                        ']');
            TableAddCol(parse_table, ParserToken.ArrayPrime, (int)ParserToken.Null,
                        (int)ParserToken.Value,
                        (int)ParserToken.ValueRest,
                        ']');

            TableAddRow(parse_table, ParserToken.Object);
            TableAddCol(parse_table, ParserToken.Object, '{',
                        '{',
                        (int)ParserToken.ObjectPrime);

            TableAddRow(parse_table, ParserToken.ObjectPrime);
            TableAddCol(parse_table, ParserToken.ObjectPrime, '"',
                        (int)ParserToken.Pair,
                        (int)ParserToken.PairRest,
                        '}');
            TableAddCol(parse_table, ParserToken.ObjectPrime, '}',
                        '}');

            TableAddRow(parse_table, ParserToken.Pair);
            TableAddCol(parse_table, ParserToken.Pair, '"',
                        (int)ParserToken.String,
                        ':',
                        (int)ParserToken.Value);

            TableAddRow(parse_table, ParserToken.PairRest);
            TableAddCol(parse_table, ParserToken.PairRest, ',',
                        ',',
                        (int)ParserToken.Pair,
                        (int)ParserToken.PairRest);
            TableAddCol(parse_table, ParserToken.PairRest, '}',
                        (int)ParserToken.Epsilon);

            TableAddRow(parse_table, ParserToken.String);
            TableAddCol(parse_table, ParserToken.String, '"',
                        '"',
                        (int)ParserToken.CharSeq,
                        '"');

            TableAddRow(parse_table, ParserToken.Text);
            TableAddCol(parse_table, ParserToken.Text, '[',
                        (int)ParserToken.Array);
            TableAddCol(parse_table, ParserToken.Text, '{',
                        (int)ParserToken.Object);

            TableAddRow(parse_table, ParserToken.Value);
            TableAddCol(parse_table, ParserToken.Value, '"',
                        (int)ParserToken.String);
            TableAddCol(parse_table, ParserToken.Value, '[',
                        (int)ParserToken.Array);
            TableAddCol(parse_table, ParserToken.Value, '{',
                        (int)ParserToken.Object);
            TableAddCol(parse_table, ParserToken.Value, (int)ParserToken.Number,
                        (int)ParserToken.Number);
            TableAddCol(parse_table, ParserToken.Value, (int)ParserToken.True,
                        (int)ParserToken.True);
            TableAddCol(parse_table, ParserToken.Value, (int)ParserToken.False,
                        (int)ParserToken.False);
            TableAddCol(parse_table, ParserToken.Value, (int)ParserToken.Null,
                        (int)ParserToken.Null);

            TableAddRow(parse_table, ParserToken.ValueRest);
            TableAddCol(parse_table, ParserToken.ValueRest, ',',
                        ',',
                        (int)ParserToken.Value,
                        (int)ParserToken.ValueRest);
            TableAddCol(parse_table, ParserToken.ValueRest, ']',
                        (int)ParserToken.Epsilon);

            return parse_table;
        }

        private static void TableAddCol(IDictionary<int, IDictionary<int, int[]>> parse_table, ParserToken row, int col,
            params int[] symbols)
        {
            parse_table[(int)row].Add(col, symbols);
        }

        private static void TableAddRow(IDictionary<int, IDictionary<int, int[]>> parse_table, ParserToken rule)
        {
            parse_table.Add((int)rule, new Dictionary<int, int[]>());
        }

        #endregion


        #region Private Methods

        private void ProcessNumber(string number)
        {
            if (number.IndexOf('.') != -1 ||
                number.IndexOf('e') != -1 ||
                number.IndexOf('E') != -1)
            {
                double n_double;
                if (double.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out n_double))
                {
                    token = JsonToken.Double;
                    token_value = n_double;

                    return;
                }
            }

            int n_int32;
            if (int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n_int32))
            {
                token = JsonToken.Int;
                token_value = n_int32;

                return;
            }

            long n_int64;
            if (long.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n_int64))
            {
                token = JsonToken.Long;
                token_value = n_int64;

                return;
            }

            ulong n_uint64;
            if (ulong.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out n_uint64))
            {
                token = JsonToken.Long;
                token_value = n_uint64;

                return;
            }

            // Shouldn't happen, but just in case, return something
            token = JsonToken.Int;
            token_value = 0;
        }

        private void ProcessSymbol()
        {
            if (current_symbol == '[')
            {
                token = JsonToken.ArrayStart;
                parser_return = true;
            }
            else if (current_symbol == ']')
            {
                token = JsonToken.ArrayEnd;
                parser_return = true;
            }
            else if (current_symbol == '{')
            {
                token = JsonToken.ObjectStart;
                parser_return = true;
            }
            else if (current_symbol == '}')
            {
                token = JsonToken.ObjectEnd;
                parser_return = true;
            }
            else if (current_symbol == '"')
            {
                if (parser_in_string)
                {
                    parser_in_string = false;

                    parser_return = true;
                }
                else
                {
                    if (token == JsonToken.None)
                    {
                        token = JsonToken.String;
                    }

                    parser_in_string = true;
                }
            }
            else if (current_symbol == (int)ParserToken.CharSeq)
            {
                token_value = lexer.StringValue;
            }
            else if (current_symbol == (int)ParserToken.False)
            {
                token = JsonToken.Boolean;
                token_value = false;
                parser_return = true;
            }
            else if (current_symbol == (int)ParserToken.Null)
            {
                token = JsonToken.Null;
                parser_return = true;
            }
            else if (current_symbol == (int)ParserToken.Number)
            {
                ProcessNumber(lexer.StringValue);

                parser_return = true;
            }
            else if (current_symbol == (int)ParserToken.Pair)
            {
                token = JsonToken.PropertyName;
            }
            else if (current_symbol == (int)ParserToken.True)
            {
                token = JsonToken.Boolean;
                token_value = true;
                parser_return = true;
            }
        }

        private bool ReadToken()
        {
            if (end_of_input)
            {
                return false;
            }

            lexer.NextToken();

            if (lexer.EndOfInput)
            {
                Close();

                return false;
            }

            current_input = lexer.Token;

            return true;
        }

        #endregion


        /// <summary>
        /// 关闭读取器并释放其持有的底层资源。
        /// </summary>
        /// <remarks>
        /// Closes the reader and releases the underlying resources it holds.
        /// </remarks>
        [Preserve]
        public void Close()
        {
            if (end_of_input)
            {
                return;
            }

            end_of_input = true;
            end_of_json = true;

            if (reader_is_owned)
            {
                using (reader)
                {
                }
            }

            reader = null;
        }

        /// <summary>
        /// 读取下一个词法单元。
        /// </summary>
        /// <remarks>
        /// Reads the next token from the JSON text.
        /// </remarks>
        /// <returns>如果成功读取下一个词法单元，则为 <c>true</c>；否则为 <c>false</c>。/ <c>true</c> if the next token was read successfully; otherwise, <c>false</c>.</returns>
        [Preserve]
        public bool Read()
        {
            if (end_of_input)
            {
                return false;
            }

            if (end_of_json)
            {
                end_of_json = false;
                automaton_stack.Clear();
                automaton_stack.Push((int)ParserToken.End);
                automaton_stack.Push((int)ParserToken.Text);
            }

            parser_in_string = false;
            parser_return = false;

            token = JsonToken.None;
            token_value = null;

            if (!read_started)
            {
                read_started = true;

                if (!ReadToken())
                {
                    return false;
                }
            }


            int[] entry_symbols;

            while (true)
            {
                if (parser_return)
                {
                    if (automaton_stack.Peek() == (int)ParserToken.End)
                    {
                        end_of_json = true;
                    }

                    return true;
                }

                current_symbol = automaton_stack.Pop();

                ProcessSymbol();

                if (current_symbol == current_input)
                {
                    if (!ReadToken())
                    {
                        if (automaton_stack.Peek() != (int)ParserToken.End)
                        {
                            throw new JsonException(
                                "Input doesn't evaluate to proper JSON text");
                        }

                        if (parser_return)
                        {
                            return true;
                        }

                        return false;
                    }

                    continue;
                }

                try
                {
                    entry_symbols =
                        parse_table[current_symbol][current_input];
                }
                catch (KeyNotFoundException e)
                {
                    throw new JsonException((ParserToken)current_input, e);
                }

                if (entry_symbols[0] == (int)ParserToken.Epsilon)
                {
                    continue;
                }

                for (var i = entry_symbols.Length - 1; i >= 0; i--)
                {
                    automaton_stack.Push(entry_symbols[i]);
                }
            }
        }
    }
}