#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using XNode;

/// <summary>
/// 节点系统文档生成器：反射扫描全部 [CreateNodeMenu] 节点，生成《节点清单.md》。
/// 菜单：Tools/节点系统/生成节点参考文档
/// 每节点：菜单路径 / 类名 / 基类 / 摘要（源码 ///）/ 端口（输入输出+类型）/ 序列化字段（名称+类型+默认值）/ 颜色。
/// </summary>
public static class NodeDocGenerator
{
    private const string OutputPath = "Assets/Scripts/Base/xNode/节点清单.md";

    [MenuItem("Tools/节点系统/生成节点参考文档")]
    public static void Generate()
    {
        List<Type> types = CollectNodeTypes();
        types.Sort((a, b) => string.Compare(GetMenuName(a), GetMenuName(b), StringComparison.Ordinal));

        var sb = new StringBuilder();
        sb.AppendLine("# 节点清单（自动生成）");
        sb.AppendLine();
        sb.AppendLine("> 由 `Tools/节点系统/生成节点参考文档` 生成。新增/修改节点后请重新生成，勿手改。");
        sb.AppendLine("> 约定：`input` / `next` 为流程通用端口，省略不列；`targetGameObject`（ComponentActionNode 目标端口）照列。");
        sb.AppendLine();

        // 目录
        var groups = types.GroupBy(t => MenuCategory(GetMenuName(t))).OrderBy(g => g.Key);
        sb.AppendLine("## 目录");
        foreach (var g in groups)
        {
            string anchor = g.Key.Replace(" ", "-");
            sb.AppendLine($"- [{g.Key}](#{anchor})");
        }
        sb.AppendLine();

        // 分类正文
        foreach (var g in groups)
        {
            sb.AppendLine($"## {g.Key}");
            sb.AppendLine();
            foreach (Type t in g)
            {
                EmitNode(sb, t);
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
        File.WriteAllText(OutputPath, sb.ToString(), new UTF8Encoding(true));
        AssetDatabase.Refresh();
        Debug.Log($"节点参考文档已生成：{OutputPath}（{types.Count} 个节点）");
    }

    private static List<Type> CollectNodeTypes()
    {
        var result = new List<Type>();
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            string asmName = asm.GetName().Name;
            if (!asmName.StartsWith("Assembly-CSharp")) continue;
            foreach (Type t in asm.GetTypes())
            {
                if (t.IsAbstract) continue;
                if (!typeof(Node).IsAssignableFrom(t)) continue;
                if (GetMenuName(t) == null) continue;
                result.Add(t);
            }
        }
        return result;
    }

    private static string GetMenuName(Type t)
    {
        Type attrType = typeof(Node.CreateNodeMenuAttribute);
        object attr = t.GetCustomAttributes(attrType, false).FirstOrDefault();
        if (attr == null) return null;
        return (string)attrType.GetField("menuName").GetValue(attr);
    }

    private static string MenuCategory(string menu) => menu.Split('/')[0];

    private static void EmitNode(StringBuilder sb, Type t)
    {
        string menu = GetMenuName(t);
        sb.AppendLine($"### {menu}（{t.Name}）");
        sb.AppendLine($"- 基类：{BaseChain(t)}");

        string summary = GetSummary(t);
        if (!string.IsNullOrEmpty(summary)) sb.AppendLine($"- 摘要：{summary}");

        string color = GetTint(t);
        if (color != null) sb.AppendLine($"- 颜色：`#{color}`");

        Node instance = (Node)ScriptableObject.CreateInstance(t);
        try
        {
            EmitPorts(sb, t, instance);
            EmitFields(sb, t, instance);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
        sb.AppendLine();
    }

    private static string BaseChain(Type t)
    {
        var names = new List<string>();
        Type cur = t.BaseType;
        while (cur != null && cur != typeof(object) && cur != typeof(ScriptableObject) && cur != typeof(Node))
        {
            names.Add(cur.Name);
            cur = cur.BaseType;
        }
        return names.Count > 0 ? string.Join(" → ", names) : t.BaseType?.Name;
    }

    private static string GetTint(Type t)
    {
        try
        {
            Type tintType = typeof(Node).Assembly.GetType("XNode.NodeTintAttribute")
                ?? typeof(Node).Assembly.GetType("XNode.Node+NodeTintAttribute");
            if (tintType == null) return null;
            object attr = t.GetCustomAttributes(tintType, false).FirstOrDefault();
            if (attr == null) return null;
            Color c = (Color)tintType.GetField("color").GetValue(attr);
            return ColorUtility.ToHtmlStringRGB(c);
        }
        catch { return null; }
    }

    private static string GetSummary(Type t)
    {
        try
        {
            var tmp = ScriptableObject.CreateInstance(t);
            MonoScript ms = MonoScript.FromScriptableObject(tmp);
            string path = AssetDatabase.GetAssetPath(ms);
            UnityEngine.Object.DestroyImmediate(tmp);
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            string[] lines = File.ReadAllLines(path);
            int idx = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("class " + t.Name + " "))
                {
                    idx = i;
                    break;
                }
            }
            if (idx < 0) return null;

            var parts = new List<string>();
            for (int i = idx - 1; i >= 0; i--)
            {
                string line = lines[i].Trim();
                if (line.StartsWith("///"))
                {
                    parts.Insert(0, line.Substring(3).Trim());
                }
                else if (string.IsNullOrEmpty(line) || line.StartsWith("[") || line.StartsWith("public "))
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            return parts.Count > 0 ? string.Join(" ", parts) : null;
        }
        catch { return null; }
    }

    private static void EmitPorts(StringBuilder sb, Type t, Node instance)
    {
        var portLines = new List<string>();
        foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (f.Name == "input" || f.Name == "next") continue;   // 流程通用端口
            bool isIn = f.GetCustomAttributes(typeof(Node.InputAttribute), false).Length > 0;
            bool isOut = f.GetCustomAttributes(typeof(Node.OutputAttribute), false).Length > 0;
            if (!isIn && !isOut) continue;

            string dir = isIn ? "输入" : "输出";
            string typeName = FormatPortType(f, instance);
            portLines.Add($"  - {dir} `{f.Name}`（{typeName}）");
        }
        if (portLines.Count == 0) return;
        sb.AppendLine("- 端口：");
        foreach (string line in portLines) sb.AppendLine(line);
    }

    private static string FormatPortType(FieldInfo f, Node instance)
    {
        try
        {
            NodePort port = instance.GetPort(f.Name);
            if (port != null && port.ValueType != null)
            {
                Type vt = port.ValueType;
                if (typeof(Node).IsAssignableFrom(vt)) return "流程";
                return PrettyType(vt);
            }
        }
        catch { }
        if (typeof(Node).IsAssignableFrom(f.FieldType)) return "流程";
        return PrettyType(f.FieldType);
    }

    private static void EmitFields(StringBuilder sb, Type t, Node instance)
    {
        var lines = new List<string>();
        foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (f.Name == "graph" || f.Name == "position" || f.Name == "ports") continue;
            if (f.Name == "input" || f.Name == "next") continue;
            if (f.GetCustomAttributes(typeof(Node.InputAttribute), false).Length > 0) continue;
            if (f.GetCustomAttributes(typeof(Node.OutputAttribute), false).Length > 0) continue;
            if (f.IsDefined(typeof(System.NonSerializedAttribute), false)) continue;

            string def;
            try
            {
                object v = f.GetValue(instance);
                def = FormatValue(v);
            }
            catch { def = ""; }
            lines.Add($"  - `{f.Name}`（{PrettyType(f.FieldType)}）= {def}");
        }
        if (lines.Count == 0) return;
        sb.AppendLine("- 字段：");
        foreach (string line in lines) sb.AppendLine(line);
    }

    private static string FormatValue(object v)
    {
        if (v == null) return "空";
        if (v is bool b) return b ? "true" : "false";
        if (v is string s) return string.IsNullOrEmpty(s) ? "\"\"" : "\"" + s + "\"";
        if (v is float f) return f.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        if (v is Vector2 v2) return $"({v2.x}, {v2.y})";
        if (v is Vector3 v3) return $"({v3.x}, {v3.y}, {v3.z})";
        if (v is Enum e) return e.ToString();
        return v.ToString();
    }

    private static string PrettyType(Type t)
    {
        if (t.IsGenericType)
        {
            string name = t.Name.Split('`')[0];
            var args = t.GetGenericArguments().Select(PrettyType);
            return $"{name}<{string.Join(",", args)}>";
        }
        if (t == typeof(GameObject)) return "GameObject";
        return t.Name;
    }
}
#endif