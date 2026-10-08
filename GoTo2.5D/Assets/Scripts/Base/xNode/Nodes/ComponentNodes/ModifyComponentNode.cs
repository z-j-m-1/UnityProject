using System;
using System.Reflection;
using UnityEngine;
using XNode;

/// <summary>组件操作模式</summary>
public enum ComponentOpMode
{
    /// <summary>添加组件（不存在时）</summary>
    Add,
    /// <summary>移除组件（存在时只销毁**组件**，不销毁物体）</summary>
    Remove
}

/// <summary>
/// 组件-添加·移除：给目标物体**动态**增删一个组件，类型用**类型名字符串**指定
/// （节点面板提供下拉，写出含命名空间的全名，避免手填拼错）。
///
/// ⚠️ **为什么不能用"拖 MonoScript"作为存储方式**：
///   <c>MonoScript</c> 属于 <c>UnityEditor</c>，而节点是运行期 ScriptableObject，
///   运行期程序集引用不到 UnityEditor —— 字段一旦用它，打包就编译不过。
///   所以存储只能用字符串 + 反射解析。
///   （不过"从 Project 拖 .cs 进节点图"这个**操作**是支持的：编辑器会据此建好一个
///     已填好类型的本节点，见 <c>BaseNodeGraphEditor.OnDropObjects</c>。）
///
/// ⚠️ **首要约束：只适用于"不需要预配置"的功能性组件**（比性能问题更关键）：
///   运行时添加的组件只有**代码里的默认值**，没有任何 Inspector 预设值。
///   所以需要配置的组件（数值 / 引用 / 资源 / 与层级相关的设置）用它加出来就是**残废的** ——
///   那种情况应当把组件**预挂在预制体上**，再用「生成/生成物体」实例化，而不是运行时添加。
///
/// ⚠️ **性能与适用场景**（别用在高频处）：
///   · <c>AddComponent</c> 要构造组件、跑 Awake/OnEnable、注册到原生侧 —— 不是免费操作；
///   · 逐帧调用还会**不断叠加**同一个组件（除非 <c>skipIfExists</c>），那是功能性 bug 而非只是慢；
///   · 类型解析只在类型名变化时走反射，结果**缓存在节点上**（稳态零反射开销）；
///   · 适合：初始化 / 生成物体 / 一次性状态切换；**逐帧需求应让组件先存在**，再用现成的组件节点驱动它。
/// </summary>
[CreateNodeMenu("组件/添加·移除")]
[NodeWidth(320)]
public class ModifyComponentNode : FlowNode
{
    [Header("目标物体（接线：任一「取值/获取物体」节点）")]
    [Input(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject targetGameObject;

    [Header("操作")]
    public ComponentOpMode mode = ComponentOpMode.Add;

    [Header("组件类型全名（用下方下拉选；仅限无需预配置的功能性组件）")]
    [Input(ShowBackingValue.Unconnected, ConnectionType.Override)]
    [Tooltip("运行时添加的组件只有代码默认值、没有 Inspector 预设值。" +
             "需要配置的组件请预挂在预制体上，用「生成/生成物体」实例化。")]
    public string componentTypeName;

    [Header("已存在时不重复添加（强烈建议开）")]
    public bool skipIfExists = true;

    [Header("输出：本次操作的那个物体（可接后续节点的目标端口）")]
    [Output(ShowBackingValue.Never)]
    [System.NonSerialized]
    public GameObject outputObject;

    // 类型解析缓存：只有类型名变了才重新解析（稳态下无反射开销）
    private Type cachedType;
    private string cachedName;

    public override void Execute()
    {
        GameObject obj = GetInputValue<GameObject>(nameof(targetGameObject), null);
        if (obj == null)
        {
            outputObject = null;
            NodeLog.Warning($"{GetType().Name}: 未接入目标物体（请把任一「取值/获取物体」节点接到目标端口）");
            return;
        }

        outputObject = obj;   // ★ 输出端口：本次操作的那个物体

        string typeName = GetInputValue<string>(nameof(componentTypeName), componentTypeName);
        if (string.IsNullOrEmpty(typeName))
        {
            NodeLog.Warning($"{GetType().Name}: 组件类型名为空（用面板下拉选择）");
            return;
        }

        Type type = ResolveType(typeName);
        if (type == null)
        {
            NodeLog.Error($"{GetType().Name}: 找不到类型 '{typeName}'（建议用面板下拉选择，写出含命名空间的全名）");
            return;
        }

        if (!typeof(Component).IsAssignableFrom(type))
        {
            NodeLog.Error($"{GetType().Name}: 类型 '{typeName}' 不是 Component，无法添加/移除");
            return;
        }

        Component existing = obj.GetComponent(type);

        if (mode == ComponentOpMode.Add)
        {
            if (existing != null)
            {
                if (skipIfExists)
                {
                    NodeLog.Verbose($"{GetType().Name}: '{obj.name}' 上已有 {type.Name}，跳过");
                    return;
                }

                NodeLog.Warning($"{GetType().Name}: '{obj.name}' 上已有 {type.Name}（skipIfExists 未开，可能重复添加）");
            }

            Component added = obj.AddComponent(type);
            if (added != null)
            {
                NodeLog.Info($"{GetType().Name}: 已给 '{obj.name}' 添加 {type.Name}");
            }
        }
        else
        {
            if (existing == null)
            {
                NodeLog.Verbose($"{GetType().Name}: '{obj.name}' 上没有 {type.Name}，无需移除");
                return;
            }

            UnityEngine.Object.Destroy(existing);   // 只销毁组件，不销毁物体
            NodeLog.Info($"{GetType().Name}: 已从 '{obj.name}' 移除 {type.Name}");
        }
    }

    /// <summary>
    /// 类型名 → Type（带缓存）。查找顺序：
    ///   1. <c>Type.GetType</c>：本节点与用户脚本同在 Assembly-CSharp，通常直接命中；也支持 "全名, 程序集名"；
    ///   2. 遍历已加载程序集 <c>Assembly.GetType</c>：支持带命名空间的全名；
    ///   3. 兜底短名扫描：兼容手填短名，有歧义会警告（建议改用下拉写出全名）。
    /// </summary>
    private Type ResolveType(string typeName)
    {
        if (cachedType != null && cachedName == typeName) return cachedType;

        Type found = Type.GetType(typeName);

        if (found == null)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try { found = asm.GetType(typeName); }
                catch { found = null; }
                if (found != null) break;
            }
        }

        if (found == null)
        {
            found = FindByShortName(typeName, out int matches);
            if (found != null && matches > 1)
            {
                NodeLog.Warning(
                    $"{GetType().Name}: 短名 '{typeName}' 匹配到 {matches} 个类型，已取 '{found.FullName}'；" +
                    "建议用面板下拉选择以便写出全名");
            }
        }

        cachedType = found;
        cachedName = typeName;
        return found;
    }

    /// <summary>兜底：按短名在所有已加载程序集里找（仅用于兼容手填短名，结果会被缓存）</summary>
    private static Type FindByShortName(string name, out int matches)
    {
        matches = 0;
        Type first = null;

        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types; }
            catch { continue; }
            if (types == null) continue;

            foreach (Type t in types)
            {
                if (t == null || t.Name != name) continue;
                matches++;
                if (first == null) first = t;
            }
        }

        return first;
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == nameof(componentTypeName))
            return GetInputValue<string>(nameof(componentTypeName), componentTypeName);

        if (port.fieldName == nameof(outputObject))
        {
            // 本节点还没执行过时回退到输入端口 —— 让输出端口不依赖执行顺序也能用
            if (outputObject == null)
                outputObject = GetInputValue<GameObject>(nameof(targetGameObject), null);
            return outputObject;
        }

        return null;
    }
}
