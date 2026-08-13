<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X LitJson

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

独立游戏开发的一站式解决方案 · 助力独立开发者实现梦想

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · [特性](#特性) · QQ 群：467608841 / 233840761

<br />

[English](README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 项目简介

一个为 Unity 改进的 **LitJson** 库，基于 [XINCGer/LitJson4Unity](https://github.com/XINCGer/LitJson4Unity) 重新打包，并为 GameFrameX 生态系统进行了扩展。

它在 [原生 LitJson](https://github.com/LitJSON/litjson) 的基础上构建，新增了对 `float` 类型、Unity 内建值类型、基于特性的序列化控制以及 IL2CPP 裁剪保护的一等支持 — 使其能够稳定应用于开启了托管代码裁剪的 Unity 项目。

> 命名空间：`GameFrameX.LitJSON.Runtime`

## 特性

### 在原生 LitJson 基础上的增强

- **`float` 类型支持** — 一等支持 `float` 的序列化（原生版本仅支持 `double`）
- **Unity 内建类型** — `Vector2` / `Vector3` / `Vector4`、`Quaternion`、`Color` / `Color32`、`Rect` / `RectOffset`、`Bounds`、`AnimationCurve`
- **[`JsonIgnoreAttribute`](#jsonignoreattribute)** — 序列化时跳过指定字段或属性
- **[`JsonPropertyAttribute`](#jsonpropertyattribute)** — 将 C# 成员映射到自定义的 JSON 键名
- **默认格式化输出** — `JsonMapper.ToJson` 默认生成美化打印的 JSON；传入 `prettyPrint: false` 可获取紧凑输出
- **类型导入/导出扩展性** — 通过 `JsonMapper.RegisterExporter<T>` 和 `RegisterImporter<TJson, TValue>` 注册自定义转换

### IL2CPP / 托管代码裁剪保护

内置三层互补的保护机制，确保库在 Unity 的 release 构建托管代码裁剪下依然可用：

- `link.xml` — 声明式类型保留
- `LitJsonCroppingHelper` — 运行时防裁剪助手，在启动时通过反射触达关键类型
- `[Preserve]` 特性 — 批量应用于对反射敏感的成员（构造函数、属性 getter、导出器），使 IL2CPP 保留它们

## 快速开始

### 安装

从以下方式中选择一种。

#### 方式 1 — Scoped Registry（推荐）

编辑 Unity 项目的 `Packages/manifest.json`：

```json
{
  "scopedRegistries": [
    {
      "name": "GameFrameX",
      "url": "https://gameframex.upm.alianblank.uk",
      "scopes": [
        "com.gameframex"
      ]
    }
  ],
  "dependencies": {
    "com.gameframex.unity.litjson": "1.1.3"
  }
}
```

仅包名以 `com.gameframex` 开头的包会通过该 registry 解析。

#### 方式 2 — 在 manifest.json 中使用 Git URL

```json
{
  "com.gameframex.unity.litjson": "https://github.com/gameframex/com.gameframex.unity.litjson.git"
}
```

#### 方式 3 — Unity Package Manager → Add package from git URL

```
https://github.com/gameframex/com.gameframex.unity.litjson.git
```

#### 方式 4 — 手动克隆

将本仓库克隆到 Unity 项目的 `Packages` 目录下，Unity 会自动识别。

### 基本用法

```csharp
using GameFrameX.LitJSON.Runtime;

class PlayerData
{
    public int Id { get; set; }

    public string Name { get; set; }

    public float Hp { get; set; }
}

// 序列化（默认美化打印）
var player = new PlayerData { Id = 42, Name = "Blank", Hp = 100.5f };
string json = JsonMapper.ToJson(player);
/*
{
    "Id"   : 42,
    "Name" : "Blank",
    "Hp"   : 100.5
}
*/

// 紧凑输出
string compact = JsonMapper.ToJson(player, false);  // {"Id":42,"Name":"Blank","Hp":100.5}

// 反序列化
PlayerData restored = JsonMapper.ToObject<PlayerData>(json);
```

<a id="jsonignoreattribute"></a>
### JsonIgnoreAttribute — 跳过成员

```csharp
using GameFrameX.LitJSON.Runtime;

class Account
{
    public string UserName { get; set; }

    [JsonIgnore]
    public string Password { get; set; }  // 序列化时被跳过
}
```

<a id="jsonpropertyattribute"></a>
### JsonPropertyAttribute — 自定义 JSON 键名

```csharp
using GameFrameX.LitJSON.Runtime;

class PlayerData
{
    [JsonProperty("player_id")]
    public int Id { get; set; }

    [JsonProperty("display_name")]
    public string Name { get; set; }

    public float Hp { get; set; }
}

// 序列化后的 JSON：
// {
//     "player_id"    : 42,
//     "display_name" : "Blank",
//     "Hp"           : 100.5
// }
```

### Unity 内建类型

Unity 值类型通过已注册的导出器进行序列化 — 无需额外特性：

```csharp
using GameFrameX.LitJSON.Runtime;
using UnityEngine;

class TransformData
{
    public Vector3 Position { get; set; }

    public Quaternion Rotation { get; set; }

    public Color Tint { get; set; }
}

var data = new TransformData
{
    Position = new Vector3(1f, 2f, 3f),
    Rotation = Quaternion.Euler(0f, 90f, 0f),
    Tint = Color.red
};

string json = JsonMapper.ToJson(data);
/*
{
    "Position" : { "x" : 1.0, "y" : 2.0, "z" : 3.0 },
    "Rotation" : { "x" : 0.0, "y" : 0.7071068, "z" : 0.0, "w" : 0.7071068 },
    "Tint"     : { "r" : 1.0, "g" : 0.0, "b" : 0.0, "a" : 1.0 }
}
*/
```

支持的 Unity 类型：`Vector2`、`Vector3`、`Vector4`、`Quaternion`、`Color`、`Color32`、`Rect`、`RectOffset`、`Bounds`、`AnimationCurve`。

### 类型扩展

注册自定义的导入/导出器以获得完全控制：

```csharp
using System;
using GameFrameX.LitJSON.Runtime;

// 写入时将 DateTime 转为 ISO-8601 字符串
JsonMapper.RegisterExporter<DateTime>((value, writer) =>
{
    writer.Write(value.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
});

// 读取时将 ISO-8601 字符串解析回 DateTime
JsonMapper.RegisterImporter<string, DateTime>(s =>
    DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind));
```

> `UnityTypeBindings.Register()` 会在启动时由 `LitJsonCroppingHelper` 自动调用；对于上面列出的内建 Unity 类型，你无需手动调用。

## 相对 XINCGer/LitJson4Unity 的改动

1. 新增 `link.xml` 用于裁剪过滤器
2. 新增 `LitJsonCroppingHelper` 防裁剪助手
3. 对反射关键成员批量应用 `[Preserve]`
4. 新增 `JsonPropertyAttribute` 用于自定义 JSON 属性名

## 文档

- [GameFrameX 文档](https://gameframex.doc.alianblank.com)
- [CHANGELOG](CHANGELOG.md)
- [LICENSE](LICENSE.md)

## 社区

- QQ 群：467608841 / 233840761

## 许可证

基于 MIT License 授权 — 详见 [LICENSE.md](LICENSE.md)。

本库是 [GameFrameX](https://github.com/AlianBlank/GameFrameX) 的子模块。
