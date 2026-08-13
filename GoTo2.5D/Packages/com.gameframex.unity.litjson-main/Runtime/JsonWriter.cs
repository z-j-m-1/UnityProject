#region Header

/**
 * JsonWriter.cs
 *   Stream-like facility to output JSON text.
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
    internal enum Condition
    {
        InArray,
        InObject,
        NotAProperty,
        Property,
        Value,
    }

    internal class WriterContext
    {
        public int Count;
        public bool InArray;
        public bool InObject;
        public bool ExpectingValue;
        public int Padding;
    }

    /// <summary>
    /// 流式 JSON 文本写入器，提供按顺序写入 JSON 标记（对象、数组、属性、值）的能力。
    /// </summary>
    /// <remarks>
    /// Stream-like facility to output JSON text. Provides sequential writing of JSON tokens
    /// (objects, arrays, properties, values) with optional validation and pretty-printing.
    /// </remarks>
    public class JsonWriter
    {
        #region Fields

        private static readonly NumberFormatInfo number_format;

        private WriterContext context;
        private Stack<WriterContext> ctx_stack;
        private bool has_reached_end;
        private char[] hex_seq;
        private int indentation;
        private int indent_value;
        private StringBuilder inst_string_builder;
        private bool pretty_print;
        private bool validate;
        private bool lower_case_properties;
        private TextWriter writer;

        #endregion


        #region Properties

        /// <summary>
        /// 获取或设置每级缩进使用的空格数。
        /// </summary>
        /// <remarks>
        /// Gets or sets the number of spaces used per indentation level.
        /// </remarks>
        /// <value>缩进空格数 / The number of spaces per indent level</value>
        [Preserve]
        public int IndentValue
        {
            get { return indent_value; }
            set
            {
                indentation = indentation / indent_value * value;
                indent_value = value;
            }
        }

        /// <summary>
        /// 获取或设置是否启用美化输出（自动换行与缩进）。
        /// </summary>
        /// <remarks>
        /// Gets or sets a value indicating whether pretty-printing
        /// (automatic newlines and indentation) is enabled.
        /// </remarks>
        /// <value>是否启用美化输出 / Whether pretty-printing is enabled</value>
        [Preserve]
        public bool PrettyPrint
        {
            get { return pretty_print; }
            set { pretty_print = value; }
        }

        /// <summary>
        /// 获取底层文本写入器实例。
        /// </summary>
        /// <remarks>
        /// Gets the underlying text writer instance.
        /// </remarks>
        /// <value>底层文本写入器 / The underlying text writer</value>
        [Preserve]
        public TextWriter TextWriter
        {
            get { return writer; }
        }

        /// <summary>
        /// 获取或设置是否启用写入验证，以检查 JSON 结构的合法性。
        /// </summary>
        /// <remarks>
        /// Gets or sets a value indicating whether write-time validation
        /// of JSON structure correctness is enabled.
        /// </remarks>
        /// <value>是否启用写入验证 / Whether validation is enabled</value>
        [Preserve]
        public bool Validate
        {
            get { return validate; }
            set { validate = value; }
        }

        /// <summary>
        /// 获取或设置是否将属性名转换为小写形式。
        /// </summary>
        /// <remarks>
        /// Gets or sets a value indicating whether property names are
        /// converted to lower case when written.
        /// </remarks>
        /// <value>是否将属性名转为小写 / Whether property names are lower-cased</value>
        [Preserve]
        public bool LowerCaseProperties
        {
            get { return lower_case_properties; }
            set { lower_case_properties = value; }
        }

        #endregion


        #region Constructors

        static JsonWriter()
        {
            number_format = NumberFormatInfo.InvariantInfo;
        }

        /// <summary>
        /// 初始化 <see cref="JsonWriter"/> 类的新实例，使用内部 <see cref="StringBuilder"/> 作为输出目标。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonWriter"/> class using an internal
        /// <see cref="StringBuilder"/> as the output target.
        /// </remarks>
        [Preserve]
        public JsonWriter()
        {
            inst_string_builder = new StringBuilder();
            writer = new StringWriter(inst_string_builder);

            Init();
        }

        /// <summary>
        /// 初始化 <see cref="JsonWriter"/> 类的新实例，写入到指定的 <see cref="StringBuilder"/>。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonWriter"/> class that writes to the
        /// specified <see cref="StringBuilder"/>.
        /// </remarks>
        /// <param name="sb">接收 JSON 输出的 <see cref="StringBuilder"/> / The <see cref="StringBuilder"/> that receives the JSON output</param>
        [Preserve]
        public JsonWriter(StringBuilder sb) :
            this(new StringWriter(sb))
        {
        }

        /// <summary>
        /// 初始化 <see cref="JsonWriter"/> 类的新实例，写入到指定的 <see cref="TextWriter"/>。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the <see cref="JsonWriter"/> class that writes to the
        /// specified <see cref="TextWriter"/>.
        /// </remarks>
        /// <param name="writer">接收 JSON 输出的 <see cref="TextWriter"/> / The <see cref="TextWriter"/> that receives the JSON output</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> 为 <c>null</c> / <paramref name="writer"/> is <c>null</c></exception>
        [Preserve]
        public JsonWriter(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            this.writer = writer;

            Init();
        }

        #endregion


        #region Private Methods

        private void DoValidation(Condition cond)
        {
            if (!context.ExpectingValue)
            {
                context.Count++;
            }

            if (!validate)
            {
                return;
            }

            if (has_reached_end)
            {
                throw new JsonException(
                    "A complete JSON symbol has already been written");
            }

            switch (cond)
            {
                case Condition.InArray:
                    if (!context.InArray)
                    {
                        throw new JsonException(
                            "Can't close an array here");
                    }

                    break;

                case Condition.InObject:
                    if (!context.InObject || context.ExpectingValue)
                    {
                        throw new JsonException(
                            "Can't close an object here");
                    }

                    break;

                case Condition.NotAProperty:
                    if (context.InObject && !context.ExpectingValue)
                    {
                        throw new JsonException(
                            "Expected a property");
                    }

                    break;

                case Condition.Property:
                    if (!context.InObject || context.ExpectingValue)
                    {
                        throw new JsonException(
                            "Can't add a property here");
                    }

                    break;

                case Condition.Value:
                    if (!context.InArray &&
                        (!context.InObject || !context.ExpectingValue))
                    {
                        throw new JsonException(
                            "Can't add a value here");
                    }

                    break;
            }
        }

        private void Init()
        {
            has_reached_end = false;
            hex_seq = new char[4];
            indentation = 0;
            indent_value = 4;
            pretty_print = true;
            validate = true;
            lower_case_properties = false;

            ctx_stack = new Stack<WriterContext>();
            context = new WriterContext();
            ctx_stack.Push(context);
        }

        private static void IntToHex(int n, char[] hex)
        {
            int num;

            for (var i = 0; i < 4; i++)
            {
                num = n % 16;

                if (num < 10)
                {
                    hex[3 - i] = (char)('0' + num);
                }
                else
                {
                    hex[3 - i] = (char)('A' + (num - 10));
                }

                n >>= 4;
            }
        }

        private void Indent()
        {
            if (pretty_print)
            {
                indentation += indent_value;
            }
        }


        private void Put(string str)
        {
            if (pretty_print && !context.ExpectingValue)
            {
                for (var i = 0; i < indentation; i++)
                {
                    writer.Write(' ');
                }
            }

            writer.Write(str);
        }

        private void PutNewline()
        {
            PutNewline(true);
        }

        private void PutNewline(bool add_comma)
        {
            if (add_comma && !context.ExpectingValue &&
                context.Count > 1)
            {
                writer.Write(',');
            }

            if (pretty_print && !context.ExpectingValue)
            {
                writer.Write(Environment.NewLine);
            }
        }

        private void PutString(string str)
        {
            Put(string.Empty);

            writer.Write('"');

            var n = str.Length;
            for (var i = 0; i < n; i++)
            {
                switch (str[i])
                {
                    case '\n':
                        writer.Write("\\n");
                        continue;

                    case '\r':
                        writer.Write("\\r");
                        continue;

                    case '\t':
                        writer.Write("\\t");
                        continue;

                    case '"':
                    case '\\':
                        writer.Write('\\');
                        writer.Write(str[i]);
                        continue;

                    case '\f':
                        writer.Write("\\f");
                        continue;

                    case '\b':
                        writer.Write("\\b");
                        continue;
                }

                if ((int)str[i] >= 32 && (int)str[i] <= 126)
                {
                    writer.Write(str[i]);
                    continue;
                }

                // Default, turn into a \uXXXX sequence
                IntToHex((int)str[i], hex_seq);
                writer.Write("\\u");
                writer.Write(hex_seq);
            }

            writer.Write('"');
        }

        private void Unindent()
        {
            if (pretty_print)
            {
                indentation -= indent_value;
            }
        }

        #endregion


        /// <summary>
        /// 返回当前已写入的 JSON 文本。仅在使用默认构造函数或传入 <see cref="StringBuilder"/> 的构造函数时有效。
        /// </summary>
        /// <remarks>
        /// Returns the JSON text that has been written so far. Only valid when the writer
        /// was constructed using the default constructor or one that accepts a <see cref="StringBuilder"/>.
        /// </remarks>
        /// <returns>已写入的 JSON 字符串；若无内部 <see cref="StringBuilder"/>，则返回 <see cref="String.Empty"/>。 / The JSON string written so far; or <see cref="String.Empty"/> if there is no internal <see cref="StringBuilder"/>.</returns>
        public override string ToString()
        {
            if (inst_string_builder == null)
            {
                return string.Empty;
            }

            return inst_string_builder.ToString();
        }

        /// <summary>
        /// 重置写入器状态，清空上下文堆栈以及已写入的内容（若内部使用 <see cref="StringBuilder"/>）。
        /// </summary>
        /// <remarks>
        /// Resets the writer state, clearing the context stack and any already-written
        /// content (when backed by an internal <see cref="StringBuilder"/>).
        /// </remarks>
        [Preserve]
        public void Reset()
        {
            has_reached_end = false;

            ctx_stack.Clear();
            context = new WriterContext();
            ctx_stack.Push(context);

            if (inst_string_builder != null)
            {
                inst_string_builder.Remove(0, inst_string_builder.Length);
            }
        }

        /// <summary>
        /// 写入布尔值（<c>true</c> 或 <c>false</c>）。
        /// </summary>
        /// <remarks>
        /// Writes a boolean value (<c>true</c> or <c>false</c>).
        /// </remarks>
        /// <param name="boolean">要写入的布尔值 / The boolean value to write</param>
        [Preserve]
        public void Write(bool boolean)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(boolean ? "true" : "false");

            context.ExpectingValue = false;
        }

        /// <summary>
        /// 写入 <see cref="decimal"/> 数值。
        /// </summary>
        /// <remarks>
        /// Writes a <see cref="decimal"/> number.
        /// </remarks>
        /// <param name="number">要写入的 decimal 数值 / The decimal number to write</param>
        [Preserve]
        public void Write(decimal number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        /// <summary>
        /// 写入双精度浮点数。若转换后的字符串不含小数点或指数标记，则追加 <c>.0</c> 以保持 JSON 数字格式。
        /// </summary>
        /// <remarks>
        /// Writes a double-precision floating-point number. Appends <c>.0</c> when the
        /// converted string lacks a decimal point or exponent, to keep valid JSON number syntax.
        /// </remarks>
        /// <param name="number">要写入的双精度浮点数 / The double value to write</param>
        [Preserve]
        public void Write(double number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            var str = Convert.ToString(number, number_format);
            Put(str);

            if (str.IndexOf('.') == -1 &&
                str.IndexOf('E') == -1)
            {
                writer.Write(".0");
            }

            context.ExpectingValue = false;
        }

        /// <summary>
        /// 写入单精度浮点数。
        /// </summary>
        /// <remarks>
        /// Writes a single-precision floating-point number.
        /// </remarks>
        /// <param name="number">要写入的单精度浮点数 / The float value to write</param>
        [Preserve]
        public void Write(float number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            var str = Convert.ToString(number, number_format);
            Put(str);

            context.ExpectingValue = false;
        }

        /// <summary>
        /// 写入 32 位整数。
        /// </summary>
        /// <remarks>
        /// Writes a 32-bit integer.
        /// </remarks>
        /// <param name="number">要写入的 32 位整数 / The 32-bit integer to write</param>
        [Preserve]
        public void Write(int number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        /// <summary>
        /// 写入 64 位长整数。
        /// </summary>
        /// <remarks>
        /// Writes a 64-bit long integer.
        /// </remarks>
        /// <param name="number">要写入的 64 位长整数 / The 64-bit long integer to write</param>
        [Preserve]
        public void Write(long number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        /// <summary>
        /// 写入字符串。若为 <c>null</c>，则写入 JSON 字面量 <c>null</c>。
        /// </summary>
        /// <remarks>
        /// Writes a string. If the value is <c>null</c>, the JSON literal <c>null</c> is written.
        /// </remarks>
        /// <param name="str">要写入的字符串 / The string to write</param>
        [Preserve]
        public void Write(string str)
        {
            DoValidation(Condition.Value);
            PutNewline();

            if (str == null)
            {
                Put("null");
            }
            else
            {
                PutString(str);
            }

            context.ExpectingValue = false;
        }

        /// <summary>
        /// 写入无符号 64 位长整数。
        /// </summary>
        /// <remarks>
        /// Writes an unsigned 64-bit long integer.
        /// </remarks>
        /// <param name="number">要写入的无符号 64 位长整数 / The unsigned 64-bit long integer to write</param>
        // [CLSCompliant(false)]
        [Preserve]
        public void Write(ulong number)
        {
            DoValidation(Condition.Value);
            PutNewline();

            Put(Convert.ToString(number, number_format));

            context.ExpectingValue = false;
        }

        /// <summary>
        /// 结束当前 JSON 数组的写入。
        /// </summary>
        /// <remarks>
        /// Ends the writing of the current JSON array.
        /// </remarks>
        [Preserve]
        public void WriteArrayEnd()
        {
            DoValidation(Condition.InArray);
            PutNewline(false);

            ctx_stack.Pop();
            if (ctx_stack.Count == 1)
            {
                has_reached_end = true;
            }
            else
            {
                context = ctx_stack.Peek();
                context.ExpectingValue = false;
            }

            Unindent();
            Put("]");
        }

        /// <summary>
        /// 开始写入一个新的 JSON 数组。
        /// </summary>
        /// <remarks>
        /// Starts writing a new JSON array.
        /// </remarks>
        [Preserve]
        public void WriteArrayStart()
        {
            DoValidation(Condition.NotAProperty);
            PutNewline();

            Put("[");

            context = new WriterContext();
            context.InArray = true;
            ctx_stack.Push(context);

            Indent();
        }

        /// <summary>
        /// 结束当前 JSON 对象的写入。
        /// </summary>
        /// <remarks>
        /// Ends the writing of the current JSON object.
        /// </remarks>
        [Preserve]
        public void WriteObjectEnd()
        {
            DoValidation(Condition.InObject);
            PutNewline(false);

            ctx_stack.Pop();
            if (ctx_stack.Count == 1)
            {
                has_reached_end = true;
            }
            else
            {
                context = ctx_stack.Peek();
                context.ExpectingValue = false;
            }

            Unindent();
            Put("}");
        }

        /// <summary>
        /// 开始写入一个新的 JSON 对象。
        /// </summary>
        /// <remarks>
        /// Starts writing a new JSON object.
        /// </remarks>
        [Preserve]
        public void WriteObjectStart()
        {
            DoValidation(Condition.NotAProperty);
            PutNewline();

            Put("{");

            context = new WriterContext();
            context.InObject = true;
            ctx_stack.Push(context);

            Indent();
        }

        /// <summary>
        /// 写入 JSON 对象的属性名。当启用 <see cref="LowerCaseProperties"/> 时，属性名会被转为小写形式。
        /// </summary>
        /// <remarks>
        /// Writes the property name of a JSON object. When <see cref="LowerCaseProperties"/>
        /// is enabled, the property name is converted to lower case.
        /// </remarks>
        /// <param name="property">要写入的属性名 / The property name to write</param>
        [Preserve]
        public void WritePropertyName(string property)
        {
            DoValidation(Condition.Property);
            PutNewline();
            var propertyName = property == null || !lower_case_properties ? property : property.ToLowerInvariant();

            PutString(propertyName);

            if (pretty_print)
            {
                if (propertyName.Length > context.Padding)
                {
                    context.Padding = propertyName.Length;
                }

                for (var i = context.Padding - propertyName.Length;
                     i >= 0;
                     i--)
                {
                    writer.Write(' ');
                }

                writer.Write(": ");
            }
            else
            {
                writer.Write(':');
            }

            context.ExpectingValue = true;
        }
    }
}