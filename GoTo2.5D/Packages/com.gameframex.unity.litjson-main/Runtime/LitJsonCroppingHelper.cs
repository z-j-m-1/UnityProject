using UnityEngine.Scripting;

namespace GameFrameX.LitJSON.Runtime
{
    /// <summary>
    /// 防止 Unity IL2CPP 构建裁剪 LitJson 类型的辅助脚本。
    /// </summary>
    /// <remarks>
    /// Helper MonoBehaviour that prevents the Unity IL2CPP build pipeline from stripping LitJson types.
    /// Works together with link.xml as a double safeguard against managed code stripping.
    /// </remarks>
    [Preserve]
    public class LitJsonCroppingHelper : UnityEngine.MonoBehaviour
    {
        /// <summary>
        /// Unity 生命周期方法，在启动时引用所有 LitJson 类型以防止被裁剪。
        /// </summary>
        /// <remarks>
        /// Unity lifecycle method invoked on startup. References every public LitJson type via <c>typeof()</c>
        /// so the IL2CPP linker retains them. The <c>[Preserve]</c> attribute and the <c>_ = typeof(...)</c>
        /// statements (C# 7.0 discard syntax) form a redundant anti-stripping mechanism alongside link.xml.
        /// </remarks>
        [Preserve]
        public void Start()
        {
            _ = typeof(ArrayMetadata);
            _ = typeof(Condition);
            _ = typeof(ExporterFunc);
            _ = typeof(ExporterFunc<>);
            _ = typeof(JsonIgnoreAttribute);
            _ = typeof(JsonPropertyAttribute);
            _ = typeof(FsmContext);
            _ = typeof(IJsonWrapper);
            _ = typeof(ImporterFunc);
            _ = typeof(ImporterFunc<,>);
            _ = typeof(JsonData);
            _ = typeof(JsonException);
            _ = typeof(JsonMapper);
            _ = typeof(JsonMockWrapper);
            _ = typeof(JsonReader);
            _ = typeof(JsonToken);
            _ = typeof(JsonType);
            _ = typeof(JsonWriter);
            _ = typeof(Lexer);
            _ = typeof(ObjectMetadata);
            _ = typeof(OrderedDictionaryEnumerator);
            _ = typeof(ParserToken);
            _ = typeof(PropertyMetadata);
            _ = typeof(UnityTypeBindings);
            _ = typeof(WrapperFactory);
            _ = typeof(WriterContext);
        }
    }
}
