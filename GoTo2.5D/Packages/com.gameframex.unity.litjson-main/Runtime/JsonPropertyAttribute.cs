// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相应法律法规及开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//   本项目采用 Apache License 2.0 单协议分发，
//   This project is licensed solely under the Apache License 2.0,
//   完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//   please refer to the LICENSE file in the root directory of the source code for the full license text.
// ==========================================================================================

using System;
using UnityEngine.Scripting;

namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 指定字段或属性在 JSON 中使用的名称。
    /// </summary>
    /// <remarks>
    /// Specifies the JSON name used for a field or property.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class JsonPropertyAttribute : Attribute
    {
        /// <summary>
        /// 初始化 JsonPropertyAttribute 的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of the JsonPropertyAttribute class.
        /// </remarks>
        /// <param name="propertyName">JSON 属性名。</param>
        [Preserve]
        public JsonPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// 获取 JSON 属性名。
        /// </summary>
        /// <remarks>
        /// Gets the JSON property name.
        /// </remarks>
        [Preserve]
        public string PropertyName { get; private set; }
    }
}
