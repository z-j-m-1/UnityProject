#region Header

/**
 * JsonMapper.cs
 *   JSON to .Net object and object to JSON conversions.
 *
 * The authors disclaim copyright to this source code. For more details, see
 * the COPYING file included with this distribution.
 **/

#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine.Scripting;


namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 描述类型的属性或字段的元数据，用于序列化与反序列化过程中。
    /// </summary>
    /// <remarks>
    /// Describes the metadata of a property or field of a type, used during serialization and deserialization.
    /// </remarks>
    internal struct PropertyMetadata
    {
        /// <summary>
        /// 成员信息（属性或字段）。
        /// </summary>
        /// <remarks>
        /// The member info (property or field).
        /// </remarks>
        public MemberInfo Info;

        /// <summary>
        /// 指示当前成员是否为字段；为 false 时表示属性。
        /// </summary>
        /// <remarks>
        /// Indicates whether the current member is a field; false means it is a property.
        /// </remarks>
        public bool IsField;

        /// <summary>
        /// 成员的类型。
        /// </summary>
        /// <remarks>
        /// The type of the member.
        /// </remarks>
        public Type Type;
    }


    /// <summary>
    /// 描述数组或列表类型在 JSON 反序列化过程中所需的元数据。
    /// </summary>
    /// <remarks>
    /// Describes the metadata required during JSON deserialization for array or list types.
    /// </remarks>
    internal struct ArrayMetadata
    {
        private Type element_type;
        private bool is_array;
        private bool is_list;


        /// <summary>
        /// 数组或列表的元素类型；未显式设置时返回 JsonData。
        /// </summary>
        /// <remarks>
        /// The element type of the array or list; returns JsonData when not explicitly set.
        /// </remarks>
        public Type ElementType
        {
            get
            {
                if (element_type == null)
                {
                    return typeof(JsonData);
                }

                return element_type;
            }

            set { element_type = value; }
        }

        /// <summary>
        /// 指示当前类型是否为数组。
        /// </summary>
        /// <remarks>
        /// Indicates whether the current type is an array.
        /// </remarks>
        public bool IsArray
        {
            get { return is_array; }
            set { is_array = value; }
        }

        /// <summary>
        /// 指示当前类型是否实现 IList 接口。
        /// </summary>
        /// <remarks>
        /// Indicates whether the current type implements the IList interface.
        /// </remarks>
        public bool IsList
        {
            get { return is_list; }
            set { is_list = value; }
        }
    }


    /// <summary>
    /// 描述对象类型（含字典）在 JSON 反序列化过程中所需的元数据。
    /// </summary>
    /// <remarks>
    /// Describes the metadata required during JSON deserialization for object types (including dictionaries).
    /// </remarks>
    internal struct ObjectMetadata
    {
        private Type element_type;
        private bool is_dictionary;

        private IDictionary<string, PropertyMetadata> properties;


        /// <summary>
        /// 字典的值元素类型；未显式设置时返回 JsonData。
        /// </summary>
        /// <remarks>
        /// The value element type of the dictionary; returns JsonData when not explicitly set.
        /// </remarks>
        public Type ElementType
        {
            get
            {
                if (element_type == null)
                {
                    return typeof(JsonData);
                }

                return element_type;
            }

            set { element_type = value; }
        }

        /// <summary>
        /// 指示当前类型是否实现 IDictionary 接口。
        /// </summary>
        /// <remarks>
        /// Indicates whether the current type implements the IDictionary interface.
        /// </remarks>
        public bool IsDictionary
        {
            get { return is_dictionary; }
            set { is_dictionary = value; }
        }

        /// <summary>
        /// 当前类型的属性与字段元数据集合，按名称索引。
        /// </summary>
        /// <remarks>
        /// The collection of property and field metadata for the current type, indexed by name.
        /// </remarks>
        public IDictionary<string, PropertyMetadata> Properties
        {
            get { return properties; }
            set { properties = value; }
        }
    }


    /// <summary>
    /// 将对象导出为 JSON 的非泛型委托，由内部导出器表使用。
    /// </summary>
    /// <remarks>
    /// Non-generic delegate that exports an object to JSON, used by the internal exporter tables.
    /// </remarks>
    /// <param name="obj">要导出的对象 / The object to export</param>
    /// <param name="writer">用于写入 JSON 数据的写入器 / The writer used to write JSON data</param>
    internal delegate void ExporterFunc(object obj, JsonWriter writer);

    /// <summary>
    /// 将指定类型的对象导出为 JSON 的委托。
    /// </summary>
    /// <remarks>
    /// Delegate that exports an object of the specified type to JSON.
    /// </remarks>
    /// <typeparam name="T">导出对象的类型 / Type of the object to export</typeparam>
    /// <param name="obj">要导出的对象 / The object to export</param>
    /// <param name="writer">用于写入 JSON 数据的写入器 / The writer used to write JSON data</param>
    public delegate void ExporterFunc<T>(T obj, JsonWriter writer);

    /// <summary>
    /// 将 JSON 输入值转换为指定目标类型的非泛型委托，由内部导入器表使用。
    /// </summary>
    /// <remarks>
    /// Non-generic delegate that converts a JSON input value to the specified target type, used by the internal importer tables.
    /// </remarks>
    /// <param name="input">要转换的 JSON 输入值 / The JSON input value to convert</param>
    /// <returns>转换后的目标值 / The converted target value</returns>
    internal delegate object ImporterFunc(object input);

    /// <summary>
    /// 将 JSON 输入值转换为指定目标类型的委托。
    /// </summary>
    /// <remarks>
    /// Delegate that converts a JSON input value to the specified target type.
    /// </remarks>
    /// <typeparam name="TJson">JSON 输入值的类型 / Type of the JSON input value</typeparam>
    /// <typeparam name="TValue">转换后目标值的类型 / Type of the converted target value</typeparam>
    /// <param name="input">要转换的 JSON 输入值 / The JSON input value to convert</param>
    /// <returns>转换后的目标值 / The converted target value</returns>
    public delegate TValue ImporterFunc<TJson, TValue>(TJson input);

    /// <summary>
    /// 创建 IJsonWrapper 实例的工厂委托。
    /// </summary>
    /// <remarks>
    /// Factory delegate that creates an IJsonWrapper instance.
    /// </remarks>
    /// <returns>新创建的 IJsonWrapper 实例 / A newly created IJsonWrapper instance</returns>
    public delegate IJsonWrapper WrapperFactory();


    /// <summary>
    /// 提供 JSON 数据的序列化（对象转 JSON）与反序列化（JSON 转对象）功能。
    /// </summary>
    /// <remarks>
    /// Provides serialization (object to JSON) and deserialization (JSON to object) functionality for JSON data.
    /// </remarks>
    public class JsonMapper
    {
        #region Fields

        private static readonly int max_nesting_depth;

        private const string UtcDateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";
        private static readonly IFormatProvider datetime_format;

        private static readonly IDictionary<Type, ExporterFunc> base_exporters_table;
        private static readonly IDictionary<Type, ExporterFunc> custom_exporters_table;

        private static readonly IDictionary<Type,
            IDictionary<Type, ImporterFunc>> base_importers_table;

        private static readonly IDictionary<Type,
            IDictionary<Type, ImporterFunc>> custom_importers_table;

        private static readonly IDictionary<Type, ArrayMetadata> array_metadata;
        private static readonly object array_metadata_lock = new();

        private static readonly object custom_table_lock = new();

        private static readonly IDictionary<Type,
            IDictionary<Type, MethodInfo>> conv_ops;

        private static readonly object conv_ops_lock = new();

        private static readonly IDictionary<Type, ObjectMetadata> object_metadata;
        private static readonly object object_metadata_lock = new();

        private static readonly IDictionary<Type,
            IList<PropertyMetadata>> type_properties;

        private static readonly object type_properties_lock = new();

        private static readonly JsonWriter static_writer;
        private static readonly object static_writer_lock = new();
        private static bool _reentrantGuard;

        #endregion


        #region Constructors

        static JsonMapper()
        {
            max_nesting_depth = 100;

            array_metadata = new Dictionary<Type, ArrayMetadata>();
            conv_ops = new Dictionary<Type, IDictionary<Type, MethodInfo>>();
            object_metadata = new Dictionary<Type, ObjectMetadata>();
            type_properties = new Dictionary<Type,
                IList<PropertyMetadata>>();

            static_writer = new JsonWriter();

            datetime_format = DateTimeFormatInfo.InvariantInfo;

            base_exporters_table = new Dictionary<Type, ExporterFunc>();
            custom_exporters_table = new Dictionary<Type, ExporterFunc>();

            base_importers_table = new Dictionary<Type,
                IDictionary<Type, ImporterFunc>>();
            custom_importers_table = new Dictionary<Type,
                IDictionary<Type, ImporterFunc>>();

            RegisterBaseExporters();
            RegisterBaseImporters();
        }

        #endregion


        #region Private Methods

        private static void AddArrayMetadata(Type type)
        {
            if (array_metadata.ContainsKey(type))
            {
                return;
            }

            var data = new ArrayMetadata();

            data.IsArray = type.IsArray;

            if (type.GetInterface("System.Collections.IList") != null)
            {
                data.IsList = true;
            }

            foreach (var p_info in type.GetProperties())
            {
                if (p_info.Name != "Item")
                {
                    continue;
                }

                var parameters = p_info.GetIndexParameters();

                if (parameters.Length != 1)
                {
                    continue;
                }

                if (parameters[0].ParameterType == typeof(int))
                {
                    data.ElementType = p_info.PropertyType;
                }
            }

            lock (array_metadata_lock)
            {
                try
                {
                    array_metadata.Add(type, data);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static void AddObjectMetadata(Type type)
        {
            if (object_metadata.ContainsKey(type))
            {
                return;
            }

            var data = new ObjectMetadata();

            if (type.GetInterface("System.Collections.IDictionary") != null)
            {
                data.IsDictionary = true;
            }

            data.Properties = new Dictionary<string, PropertyMetadata>();

            foreach (var p_info in type.GetProperties())
            {
                if (p_info.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length > 0)
                {
                    continue;
                }

                if (p_info.Name == "Item")
                {
                    var parameters = p_info.GetIndexParameters();

                    if (parameters.Length != 1)
                    {
                        continue;
                    }

                    if (parameters[0].ParameterType == typeof(string))
                    {
                        data.ElementType = p_info.PropertyType;
                    }

                    continue;
                }

                var p_data = new PropertyMetadata();
                p_data.Info = p_info;
                p_data.Type = p_info.PropertyType;

                data.Properties.Add(GetJsonPropertyName(p_info), p_data);
            }

            foreach (var f_info in type.GetFields())
            {
                if (f_info.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length > 0)
                {
                    continue;
                }

                var p_data = new PropertyMetadata();
                p_data.Info = f_info;
                p_data.IsField = true;
                p_data.Type = f_info.FieldType;

                data.Properties.Add(GetJsonPropertyName(f_info), p_data);
            }

            lock (object_metadata_lock)
            {
                try
                {
                    object_metadata.Add(type, data);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static void AddTypeProperties(Type type)
        {
            if (type_properties.ContainsKey(type))
            {
                return;
            }

            IList<PropertyMetadata> props = new List<PropertyMetadata>();

            foreach (var p_info in type.GetProperties())
            {
                if (p_info.Name == "Item")
                {
                    continue;
                }

                var p_data = new PropertyMetadata();
                p_data.Info = p_info;
                p_data.IsField = false;
                props.Add(p_data);
            }

            foreach (var f_info in type.GetFields())
            {
                var p_data = new PropertyMetadata();
                p_data.Info = f_info;
                p_data.IsField = true;

                props.Add(p_data);
            }

            lock (type_properties_lock)
            {
                try
                {
                    type_properties.Add(type, props);
                }
                catch (ArgumentException)
                {
                    return;
                }
            }
        }

        private static MethodInfo GetConvOp(Type t1, Type t2)
        {
            lock (conv_ops_lock)
            {
                if (!conv_ops.ContainsKey(t1))
                {
                    conv_ops.Add(t1, new Dictionary<Type, MethodInfo>());
                }

                if (conv_ops[t1].ContainsKey(t2))
                {
                    return conv_ops[t1][t2];
                }
            }

            var op = t1.GetMethod(
                "op_Implicit", new Type[] { t2, });

            lock (conv_ops_lock)
            {
                try
                {
                    conv_ops[t1].Add(t2, op);
                }
                catch (ArgumentException)
                {
                    return conv_ops[t1][t2];
                }
            }

            return op;
        }

        private static string GetJsonPropertyName(MemberInfo info)
        {
            var attributes = info.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
            if (attributes.Length == 0)
            {
                return info.Name;
            }

            var attribute = (JsonPropertyAttribute)attributes[0];
            return string.IsNullOrEmpty(attribute.PropertyName) ? info.Name : attribute.PropertyName;
        }

        private static bool TryGetProperty(ObjectMetadata metadata, string name,
            out PropertyMetadata prop)
        {
            if (metadata.Properties.TryGetValue(name, out prop))
            {
                return true;
            }

            // 精确匹配失败时按 OrdinalIgnoreCase 回退，兼容 JSON 键与成员名 /
            // JsonProperty 大小写不一致的场景（例如 PascalCase 的 JSON 对应
            // camelCase 的属性）。精确匹配的语义保持不变，仅原本会被跳过的字段获益。
            foreach (var pair in metadata.Properties)
            {
                if (string.Equals(pair.Key, name,
                    StringComparison.OrdinalIgnoreCase))
                {
                    prop = pair.Value;
                    return true;
                }
            }

            prop = default(PropertyMetadata);
            return false;
        }

        private static object ReadValue(Type inst_type, JsonReader reader)
        {
            reader.Read();

            if (reader.Token == JsonToken.ArrayEnd)
            {
                return null;
            }

            var underlying_type = Nullable.GetUnderlyingType(inst_type);
            var value_type = underlying_type ?? inst_type;

            if (reader.Token == JsonToken.Null)
            {
#if NETSTANDARD1_5
                if (inst_type.IsClass() || underlying_type != null) {
                    return null;
                }
#else
                if (inst_type.IsClass || underlying_type != null)
                {
                    return null;
                }
#endif

                throw new JsonException(string.Format(
                                            "Can't assign null to an instance of type {0}",
                                            inst_type));
            }

            if (reader.Token == JsonToken.Double ||
                reader.Token == JsonToken.Int ||
                reader.Token == JsonToken.Long ||
                reader.Token == JsonToken.String ||
                reader.Token == JsonToken.Boolean)
            {
                var json_type = reader.Value.GetType();

                if (value_type.IsAssignableFrom(json_type))
                {
                    return reader.Value;
                }

                // If there's a custom importer that fits, use it
                lock (custom_table_lock)
                {
                    if (custom_importers_table.ContainsKey(json_type) &&
                        custom_importers_table[json_type].ContainsKey(
                            value_type))
                    {
                        var importer =
                            custom_importers_table[json_type][value_type];

                        return importer(reader.Value);
                    }
                }

                // Maybe there's a base importer that works
                if (base_importers_table.ContainsKey(json_type) &&
                    base_importers_table[json_type].ContainsKey(
                        value_type))
                {
                    var importer =
                        base_importers_table[json_type][value_type];

                    return importer(reader.Value);
                }

                // Maybe it's an enum
#if NETSTANDARD1_5
                if (value_type.IsEnum())
                {
                    return Enum.ToObject(value_type, reader.Value);
                }
#else
                if (value_type.IsEnum)
                {
                    return Enum.ToObject(value_type, reader.Value);
                }
#endif
                // Try using an implicit conversion operator
                var conv_op = GetConvOp(value_type, json_type);

                if (conv_op != null)
                {
                    return conv_op.Invoke(null,
                                          new object[] { reader.Value, });
                }

                // No luck
                throw new JsonException(string.Format(
                                            "Can't assign value '{0}' (type {1}) to type {2}",
                                            reader.Value, json_type, inst_type));
            }

            object instance = null;

            if (reader.Token == JsonToken.ArrayStart)
            {
                AddArrayMetadata(inst_type);
                var t_data = array_metadata[inst_type];

                if (!t_data.IsArray && !t_data.IsList)
                {
                    throw new JsonException(string.Format(
                                                "Type {0} can't act as an array",
                                                inst_type));
                }

                IList list;
                Type elem_type;

                if (!t_data.IsArray)
                {
                    list = (IList)Activator.CreateInstance(inst_type);
                    elem_type = t_data.ElementType;
                }
                else
                {
                    list = new ArrayList();
                    elem_type = inst_type.GetElementType();
                }

                while (true)
                {
                    var item = ReadValue(elem_type, reader);
                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                    {
                        break;
                    }

                    list.Add(item);
                }

                if (t_data.IsArray)
                {
                    var n = list.Count;
                    instance = Array.CreateInstance(elem_type, n);

                    for (var i = 0; i < n; i++)
                    {
                        ((Array)instance).SetValue(list[i], i);
                    }
                }
                else
                {
                    instance = list;
                }
            }
            else if (reader.Token == JsonToken.ObjectStart)
            {
                AddObjectMetadata(value_type);
                var t_data = object_metadata[value_type];

                instance = Activator.CreateInstance(value_type);

                while (true)
                {
                    reader.Read();

                    if (reader.Token == JsonToken.ObjectEnd)
                    {
                        break;
                    }

                    var property = (string)reader.Value;

                    if (TryGetProperty(t_data, property, out var prop_data))
                    {
                        if (prop_data.IsField)
                        {
                            ((FieldInfo)prop_data.Info).SetValue(
                                instance, ReadValue(prop_data.Type, reader));
                        }
                        else
                        {
                            var p_info =
                                (PropertyInfo)prop_data.Info;

                            if (p_info.GetSetMethod(false) != null)
                            {
                                p_info.SetValue(
                                    instance,
                                    ReadValue(prop_data.Type, reader),
                                    null);
                            }
                            else
                            {
                                ReadValue(prop_data.Type, reader);
                            }
                        }
                    }
                    else
                    {
                        if (!t_data.IsDictionary)
                        {
                            if (!reader.SkipNonMembers)
                            {
                                throw new JsonException(string.Format(
                                                            "The type {0} doesn't have the " +
                                                            "property '{1}'",
                                                            inst_type, property));
                            }
                            else
                            {
                                ReadSkip(reader);
                                continue;
                            }
                        }

                        ((IDictionary)instance).Add(
                            property, ReadValue(
                                t_data.ElementType, reader));
                    }
                }
            }

            return instance;
        }

        private static IJsonWrapper ReadValue(WrapperFactory factory,
            JsonReader reader)
        {
            reader.Read();

            if (reader.Token == JsonToken.ArrayEnd ||
                reader.Token == JsonToken.Null)
            {
                return null;
            }

            var instance = factory();

            if (reader.Token == JsonToken.String)
            {
                instance.SetString((string)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Double)
            {
                instance.SetDouble((double)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Int)
            {
                instance.SetInt((int)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Long)
            {
                instance.SetLong((long)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.Boolean)
            {
                instance.SetBoolean((bool)reader.Value);
                return instance;
            }

            if (reader.Token == JsonToken.ArrayStart)
            {
                instance.SetJsonType(JsonType.Array);

                while (true)
                {
                    var item = ReadValue(factory, reader);
                    if (item == null && reader.Token == JsonToken.ArrayEnd)
                    {
                        break;
                    }

                    ((IList)instance).Add(item);
                }
            }
            else if (reader.Token == JsonToken.ObjectStart)
            {
                instance.SetJsonType(JsonType.Object);

                while (true)
                {
                    reader.Read();

                    if (reader.Token == JsonToken.ObjectEnd)
                    {
                        break;
                    }

                    var property = (string)reader.Value;

                    ((IDictionary)instance)[property] = ReadValue(
                        factory, reader);
                }
            }

            return instance;
        }

        private static void ReadSkip(JsonReader reader)
        {
            ToWrapper(
                delegate { return new JsonMockWrapper(); }, reader);
        }

        private static void RegisterBaseExporters()
        {
            base_exporters_table[typeof(byte)] =
                delegate(object obj, JsonWriter writer) { writer.Write(Convert.ToInt32((byte)obj)); };

            base_exporters_table[typeof(char)] =
                delegate(object obj, JsonWriter writer) { writer.Write(Convert.ToString((char)obj)); };

            base_exporters_table[typeof(DateTime)] =
                delegate(object obj, JsonWriter writer)
                {
                    writer.Write(((DateTime)obj).ToUniversalTime().ToString(
                                     UtcDateTimeFormat,
                                     datetime_format));
                };

            base_exporters_table[typeof(decimal)] =
                delegate(object obj, JsonWriter writer) { writer.Write((decimal)obj); };

            base_exporters_table[typeof(sbyte)] =
                delegate(object obj, JsonWriter writer) { writer.Write(Convert.ToInt32((sbyte)obj)); };

            base_exporters_table[typeof(short)] =
                delegate(object obj, JsonWriter writer) { writer.Write(Convert.ToInt32((short)obj)); };

            base_exporters_table[typeof(ushort)] =
                delegate(object obj, JsonWriter writer) { writer.Write(Convert.ToInt32((ushort)obj)); };

            base_exporters_table[typeof(uint)] =
                delegate(object obj, JsonWriter writer) { writer.Write(Convert.ToUInt64((uint)obj)); };

            base_exporters_table[typeof(ulong)] =
                delegate(object obj, JsonWriter writer) { writer.Write((ulong)obj); };

            base_exporters_table[typeof(DateTimeOffset)] =
                delegate(object obj, JsonWriter writer) { writer.Write(((DateTimeOffset)obj).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", datetime_format)); };
        }

        private static void RegisterBaseImporters()
        {
            ImporterFunc importer;

            importer = delegate(object input) { return Convert.ToByte((int)input); };
            RegisterImporter(base_importers_table, typeof(int),
                             typeof(byte), importer);

            importer = delegate(object input) { return Convert.ToUInt64((int)input); };
            RegisterImporter(base_importers_table, typeof(int),
                             typeof(ulong), importer);

            importer = delegate(object input) { return Convert.ToInt64((int)input); };
            RegisterImporter(base_importers_table, typeof(int),
                             typeof(long), importer);

            importer = delegate(object input) { return Convert.ToSByte((int)input); };
            RegisterImporter(base_importers_table, typeof(int),
                             typeof(sbyte), importer);

            importer = delegate(object input) { return Convert.ToInt16((int)input); };
            RegisterImporter(base_importers_table, typeof(int),
                             typeof(short), importer);

            importer = delegate(object input) { return Convert.ToUInt16((int)input); };
            RegisterImporter(base_importers_table, typeof(int),
                             typeof(ushort), importer);

            importer = delegate(object input) { return Convert.ToUInt32((int)input); };
            RegisterImporter(base_importers_table, typeof(int),
                             typeof(uint), importer);

            importer = delegate(object input) { return Convert.ToSingle((int)input); };
            RegisterImporter(base_importers_table, typeof(int),
                             typeof(float), importer);

            importer = delegate(object input) { return Convert.ToDouble((int)input); };
            RegisterImporter(base_importers_table, typeof(int),
                             typeof(double), importer);

            importer = delegate(object input) { return Convert.ToDecimal((double)input); };
            RegisterImporter(base_importers_table, typeof(double),
                             typeof(decimal), importer);

            importer = delegate(object input) { return Convert.ToSingle((double)input); };
            RegisterImporter(base_importers_table, typeof(double),
                             typeof(float), importer);

            importer = delegate(object input) { return Convert.ToUInt32((long)input); };
            RegisterImporter(base_importers_table, typeof(long),
                             typeof(uint), importer);

            importer = delegate(object input) { return Convert.ToChar((string)input); };
            RegisterImporter(base_importers_table, typeof(string),
                             typeof(char), importer);

            importer = delegate(object input)
            {
                return DateTime.Parse((string)input,
                                      datetime_format,
                                      DateTimeStyles.AdjustToUniversal |
                                      DateTimeStyles.AssumeUniversal);
            };
            RegisterImporter(base_importers_table, typeof(string),
                             typeof(DateTime), importer);

            importer = delegate(object input) { return DateTimeOffset.Parse((string)input, datetime_format); };
            RegisterImporter(base_importers_table, typeof(string),
                             typeof(DateTimeOffset), importer);
        }

        private static void RegisterImporter(
            IDictionary<Type, IDictionary<Type, ImporterFunc>> table,
            Type json_type, Type value_type, ImporterFunc importer)
        {
            if (!table.ContainsKey(json_type))
            {
                table.Add(json_type, new Dictionary<Type, ImporterFunc>());
            }

            table[json_type][value_type] = importer;
        }

        private static void WriteValue(object obj, JsonWriter writer,
            bool writer_is_private,
            int depth)
        {
            if (depth > max_nesting_depth)
            {
                throw new JsonException(
                    string.Format("Max allowed object depth reached while " +
                                  "trying to export from type {0}",
                                  obj.GetType()));
            }

            if (obj == null)
            {
                writer.Write(null);
                return;
            }

            if (obj is IJsonWrapper)
            {
                if (writer_is_private)
                {
                    writer.TextWriter.Write(((IJsonWrapper)obj).ToJson());
                }
                else
                {
                    ((IJsonWrapper)obj).ToJson(writer);
                }

                return;
            }

            if (obj is string)
            {
                writer.Write((string)obj);
                return;
            }

            if (obj is double)
            {
                writer.Write((double)obj);
                return;
            }

            if (obj is float)
            {
                writer.Write((float)obj);
                return;
            }

            if (obj is int)
            {
                writer.Write((int)obj);
                return;
            }

            if (obj is bool)
            {
                writer.Write((bool)obj);
                return;
            }

            if (obj is long)
            {
                writer.Write((long)obj);
                return;
            }

            if (obj is Array)
            {
                writer.WriteArrayStart();

                foreach (var elem in (Array)obj)
                {
                    WriteValue(elem, writer, writer_is_private, depth + 1);
                }

                writer.WriteArrayEnd();

                return;
            }

            if (obj is IList)
            {
                writer.WriteArrayStart();
                foreach (var elem in (IList)obj)
                {
                    WriteValue(elem, writer, writer_is_private, depth + 1);
                }

                writer.WriteArrayEnd();

                return;
            }

#if UNITY_2018_3_OR_NEWER
            if (obj is IDictionary dictionary)
            {
#else
            if (obj is IDictionary)
            {
                var dictionary = obj as IDictionary;
#endif
                writer.WriteObjectStart();
                foreach (DictionaryEntry entry in dictionary)
                {
#if UNITY_2018_3_OR_NEWER
                    var propertyName = entry.Key is string key
                                           ? key
                                           : Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
#else
                    var propertyName = entry.Key is string ? (entry.Key as string) : Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
#endif
                    writer.WritePropertyName(propertyName);
                    WriteValue(entry.Value, writer, writer_is_private,
                               depth + 1);
                }

                writer.WriteObjectEnd();

                return;
            }

            var obj_type = obj.GetType();

            // See if there's a custom exporter for the object
            lock (custom_table_lock)
            {
                if (custom_exporters_table.ContainsKey(obj_type))
                {
                    var exporter = custom_exporters_table[obj_type];
                    exporter(obj, writer);

                    return;
                }
            }

            // If not, maybe there's a base exporter
            if (base_exporters_table.ContainsKey(obj_type))
            {
                var exporter = base_exporters_table[obj_type];
                exporter(obj, writer);

                return;
            }

            // Last option, let's see if it's an enum
            if (obj is Enum)
            {
                var e_type = Enum.GetUnderlyingType(obj_type);

                if (e_type == typeof(long)
                    || e_type == typeof(uint)
                    || e_type == typeof(ulong))
                {
                    writer.Write((ulong)obj);
                }
                else
                {
                    writer.Write((int)obj);
                }

                return;
            }

            // Okay, so it looks like the input should be exported as an
            // object
            AddTypeProperties(obj_type);
            var props = type_properties[obj_type];

            writer.WriteObjectStart();
            foreach (var p_data in props)
            {
                var skipAttributesList = p_data.Info.GetCustomAttributes(typeof(JsonIgnoreAttribute), true);
                if (skipAttributesList.Length > 0)
                {
                    continue;
                }

                if (p_data.IsField)
                {
                    writer.WritePropertyName(GetJsonPropertyName(p_data.Info));
                    WriteValue(((FieldInfo)p_data.Info).GetValue(obj),
                               writer, writer_is_private, depth + 1);
                }
                else
                {
                    var p_info = (PropertyInfo)p_data.Info;

                    if (p_info.CanRead)
                    {
                        writer.WritePropertyName(GetJsonPropertyName(p_data.Info));
                        WriteValue(p_info.GetValue(obj, null),
                                   writer, writer_is_private, depth + 1);
                    }
                }
            }

            writer.WriteObjectEnd();
        }

        #endregion


        /// <summary>
        /// 将对象序列化为 JSON 字符串。
        /// </summary>
        /// <remarks>
        /// Serializes an object to a JSON string.
        /// </remarks>
        /// <param name="obj">要序列化的对象 / The object to serialize</param>
        /// <param name="prettyPrint">是否启用美化输出（缩进与换行） / Whether to enable pretty-printed output (indentation and line breaks)</param>
        /// <returns>序列化后的 JSON 字符串 / The serialized JSON string</returns>
        /// <exception cref="InvalidOperationException">在导出器或 ToString() 内部递归调用本方法时抛出 / Thrown when this method is called recursively from within an exporter or ToString()</exception>
        [Preserve]
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            lock (static_writer_lock)
            {
                if (_reentrantGuard)
                {
                    throw new InvalidOperationException(
                        "JsonMapper.ToJson() cannot be called recursively from within an exporter or ToString(). " +
                        "Use ToJson(object, JsonWriter) overload instead.");
                }

                _reentrantGuard = true;
                try
                {
                    static_writer.PrettyPrint = prettyPrint;

                    static_writer.Reset();

                    WriteValue(obj, static_writer, true, 0);

                    return static_writer.ToString();
                }
                finally
                {
                    _reentrantGuard = false;
                }
            }
        }

        /// <summary>
        /// 将对象序列化到指定的 JsonWriter。
        /// </summary>
        /// <remarks>
        /// Serializes an object to the specified JsonWriter.
        /// </remarks>
        /// <param name="obj">要序列化的对象 / The object to serialize</param>
        /// <param name="writer">用于写入 JSON 数据的写入器 / The writer used to write JSON data</param>
        [Preserve]
        public static void ToJson(object obj, JsonWriter writer)
        {
            WriteValue(obj, writer, false, 0);
        }

        /// <summary>
        /// 将 JsonReader 中的内容反序列化为 JsonData。
        /// </summary>
        /// <remarks>
        /// Deserializes the content from a JsonReader into a JsonData instance.
        /// </remarks>
        /// <param name="reader">提供 JSON 数据的读取器 / The reader that provides the JSON data</param>
        /// <returns>反序列化得到的 JsonData / The deserialized JsonData</returns>
        [Preserve]
        public static JsonData ToObject(JsonReader reader)
        {
            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, reader);
        }

        /// <summary>
        /// 将 TextReader 中的内容反序列化为 JsonData。
        /// </summary>
        /// <remarks>
        /// Deserializes the content from a TextReader into a JsonData instance.
        /// </remarks>
        /// <param name="reader">提供 JSON 文本的读取器 / The text reader that provides the JSON text</param>
        /// <returns>反序列化得到的 JsonData / The deserialized JsonData</returns>
        [Preserve]
        public static JsonData ToObject(TextReader reader)
        {
            var json_reader = new JsonReader(reader);

            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, json_reader);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为 JsonData。
        /// </summary>
        /// <remarks>
        /// Deserializes a JSON string into a JsonData instance.
        /// </remarks>
        /// <param name="json">要反序列化的 JSON 字符串 / The JSON string to deserialize</param>
        /// <returns>反序列化得到的 JsonData / The deserialized JsonData</returns>
        [Preserve]
        public static JsonData ToObject(string json)
        {
            return (JsonData)ToWrapper(
                delegate { return new JsonData(); }, json);
        }

        /// <summary>
        /// 将 JsonReader 中的内容反序列化为指定类型的对象。
        /// </summary>
        /// <remarks>
        /// Deserializes the content from a JsonReader into an object of the specified type.
        /// </remarks>
        /// <typeparam name="T">目标类型 / The target type</typeparam>
        /// <param name="reader">提供 JSON 数据的读取器 / The reader that provides the JSON data</param>
        /// <returns>反序列化得到的对象 / The deserialized object</returns>
        [Preserve]
        public static T ToObject<T>(JsonReader reader)
        {
            return (T)ReadValue(typeof(T), reader);
        }

        /// <summary>
        /// 将 TextReader 中的内容反序列化为指定类型的对象。
        /// </summary>
        /// <remarks>
        /// Deserializes the content from a TextReader into an object of the specified type.
        /// </remarks>
        /// <typeparam name="T">目标类型 / The target type</typeparam>
        /// <param name="reader">提供 JSON 文本的读取器 / The text reader that provides the JSON text</param>
        /// <returns>反序列化得到的对象 / The deserialized object</returns>
        [Preserve]
        public static T ToObject<T>(TextReader reader)
        {
            var json_reader = new JsonReader(reader);

            return (T)ReadValue(typeof(T), json_reader);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的对象。
        /// </summary>
        /// <remarks>
        /// Deserializes a JSON string into an object of the specified type.
        /// </remarks>
        /// <typeparam name="T">目标类型 / The target type</typeparam>
        /// <param name="json">要反序列化的 JSON 字符串 / The JSON string to deserialize</param>
        /// <returns>反序列化得到的对象 / The deserialized object</returns>
        [Preserve]
        public static T ToObject<T>(string json)
        {
            var reader = new JsonReader(json);

            return (T)ReadValue(typeof(T), reader);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定 Type 的对象。
        /// </summary>
        /// <remarks>
        /// Deserializes a JSON string into an object of the specified Type.
        /// </remarks>
        /// <param name="json">要反序列化的 JSON 字符串 / The JSON string to deserialize</param>
        /// <param name="ConvertType">目标类型 / The target type</param>
        /// <returns>反序列化得到的对象 / The deserialized object</returns>
        [Preserve]
        public static object ToObject(string json, Type ConvertType)
        {
            var reader = new JsonReader(json);

            return ReadValue(ConvertType, reader);
        }

        /// <summary>
        /// 使用自定义工厂将 JsonReader 中的内容反序列化为 IJsonWrapper。
        /// </summary>
        /// <remarks>
        /// Deserializes the content from a JsonReader into an IJsonWrapper using a custom factory.
        /// </remarks>
        /// <param name="factory">用于创建 IJsonWrapper 实例的工厂委托 / The factory delegate used to create IJsonWrapper instances</param>
        /// <param name="reader">提供 JSON 数据的读取器 / The reader that provides the JSON data</param>
        /// <returns>反序列化得到的 IJsonWrapper / The deserialized IJsonWrapper</returns>
        [Preserve]
        public static IJsonWrapper ToWrapper(WrapperFactory factory,
            JsonReader reader)
        {
            return ReadValue(factory, reader);
        }

        /// <summary>
        /// 使用自定义工厂将 JSON 字符串反序列化为 IJsonWrapper。
        /// </summary>
        /// <remarks>
        /// Deserializes a JSON string into an IJsonWrapper using a custom factory.
        /// </remarks>
        /// <param name="factory">用于创建 IJsonWrapper 实例的工厂委托 / The factory delegate used to create IJsonWrapper instances</param>
        /// <param name="json">要反序列化的 JSON 字符串 / The JSON string to deserialize</param>
        /// <returns>反序列化得到的 IJsonWrapper / The deserialized IJsonWrapper</returns>
        [Preserve]
        public static IJsonWrapper ToWrapper(WrapperFactory factory,
            string json)
        {
            var reader = new JsonReader(json);

            return ReadValue(factory, reader);
        }

        /// <summary>
        /// 为指定类型注册自定义导出器，控制该类型对象序列化为 JSON 的方式。
        /// </summary>
        /// <remarks>
        /// Registers a custom exporter for the specified type, controlling how objects of that type are serialized to JSON.
        /// </remarks>
        /// <typeparam name="T">要注册导出器的类型 / The type to register the exporter for</typeparam>
        /// <param name="exporter">执行导出逻辑的委托 / The delegate that performs the export logic</param>
        [Preserve]
        public static void RegisterExporter<T>(ExporterFunc<T> exporter)
        {
            ExporterFunc exporter_wrapper =
                delegate(object obj, JsonWriter writer) { exporter((T)obj, writer); };

            lock (custom_table_lock)
            {
                custom_exporters_table[typeof(T)] = exporter_wrapper;
            }
        }

        /// <summary>
        /// 注册自定义导入器，控制 JSON 值向指定目标类型的转换方式。
        /// </summary>
        /// <remarks>
        /// Registers a custom importer, controlling how JSON values are converted to the specified target type.
        /// </remarks>
        /// <typeparam name="TJson">JSON 输入值的类型 / Type of the JSON input value</typeparam>
        /// <typeparam name="TValue">转换后目标值的类型 / Type of the converted target value</typeparam>
        /// <param name="importer">执行转换逻辑的委托 / The delegate that performs the conversion logic</param>
        [Preserve]
        public static void RegisterImporter<TJson, TValue>(
            ImporterFunc<TJson, TValue> importer)
        {
            ImporterFunc importer_wrapper =
                delegate(object input) { return importer((TJson)input); };

            lock (custom_table_lock)
            {
                RegisterImporter(custom_importers_table, typeof(TJson),
                                 typeof(TValue), importer_wrapper);
            }
        }

        /// <summary>
        /// 清除所有已注册的自定义导出器。
        /// </summary>
        /// <remarks>
        /// Removes all registered custom exporters.
        /// </remarks>
        [Preserve]
        public static void UnregisterExporters()
        {
            lock (custom_table_lock)
            {
                custom_exporters_table.Clear();
            }
        }

        /// <summary>
        /// 清除所有已注册的自定义导入器。
        /// </summary>
        /// <remarks>
        /// Removes all registered custom importers.
        /// </remarks>
        [Preserve]
        public static void UnregisterImporters()
        {
            lock (custom_table_lock)
            {
                custom_importers_table.Clear();
            }
        }
    }
}
