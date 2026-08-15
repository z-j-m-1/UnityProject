using System.Text.RegularExpressions;

/// <summary>
/// 富文本工具
/// </summary>
public static class RichTextUtility
{
    private static readonly Regex richTextRegex = new Regex("<.*?>", RegexOptions.Compiled);

    /// <summary>剔除 HTML/XML 富文本标签</summary>
    public static string Strip(string content)
    {
        if (string.IsNullOrEmpty(content)) return content ?? string.Empty;
        return richTextRegex.Replace(content, string.Empty);
    }
}
