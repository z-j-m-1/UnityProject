<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X LitJson

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#quick-start) · [Features](#features) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## Project Overview

An improved **LitJson** library for Unity, repackaged from [XINCGer/LitJson4Unity](https://github.com/XINCGer/LitJson4Unity) and extended for the GameFrameX ecosystem.

It builds on top of the [original LitJson](https://github.com/LitJSON/litjson) and adds first-class support for the `float` type, Unity built-in value types, attribute-driven serialization control, and IL2CPP stripping protection — making it production-ready for Unity projects that ship with managed code stripping enabled.

> Namespace: `GameFrameX.LitJSON.Runtime`

## Features

### Beyond original LitJson

- **`float` type support** — first-class serialization of `float` (the original only handled `double`)
- **Unity built-in types** — `Vector2` / `Vector3` / `Vector4`, `Quaternion`, `Color` / `Color32`, `Rect` / `RectOffset`, `Bounds`, `AnimationCurve`
- **[`JsonIgnoreAttribute`](#jsonignoreattribute)** — skip a field or property during serialization
- **[`JsonPropertyAttribute`](#jsonpropertyattribute)** — map a C# member to a custom JSON key
- **Formatted output by default** — `JsonMapper.ToJson` produces pretty-printed JSON out of the box; pass `prettyPrint: false` for compact output
- **Type importer / exporter extensibility** — register custom conversions via `JsonMapper.RegisterExporter<T>` and `RegisterImporter<TJson, TValue>`

### IL2CPP / managed stripping safety

Ships with three complementary safeguards so the library survives Unity's managed code stripping in release builds:

- `link.xml` — declarative type preservation
- `LitJsonCroppingHelper` — runtime anti-stripping helper that touches critical types via reflection on startup
- `[Preserve]` attributes — batch-applied on reflection-sensitive members (constructors, property getters, exporters) so IL2CPP keeps them

## Quick Start

### Installation

Choose one of the methods below.

#### Option 1 — Scoped registry (recommended)

Edit your Unity project's `Packages/manifest.json`:

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

Only packages whose names start with `com.gameframex` resolve through this registry.

#### Option 2 — Direct Git URL in manifest.json

```json
{
  "com.gameframex.unity.litjson": "https://github.com/gameframex/com.gameframex.unity.litjson.git"
}
```

#### Option 3 — Unity Package Manager → Add package from git URL

```
https://github.com/gameframex/com.gameframex.unity.litjson.git
```

#### Option 4 — Manual clone

Clone this repository into your Unity project's `Packages` directory; Unity will pick it up automatically.

### Basic usage

```csharp
using GameFrameX.LitJSON.Runtime;

class PlayerData
{
    public int Id { get; set; }

    public string Name { get; set; }

    public float Hp { get; set; }
}

// Serialize (pretty-printed by default)
var player = new PlayerData { Id = 42, Name = "Blank", Hp = 100.5f };
string json = JsonMapper.ToJson(player);
/*
{
    "Id"   : 42,
    "Name" : "Blank",
    "Hp"   : 100.5
}
*/

// Compact output
string compact = JsonMapper.ToJson(player, false);  // {"Id":42,"Name":"Blank","Hp":100.5}

// Deserialize
PlayerData restored = JsonMapper.ToObject<PlayerData>(json);
```

<a id="jsonignoreattribute"></a>
### JsonIgnoreAttribute — skip a member

```csharp
using GameFrameX.LitJSON.Runtime;

class Account
{
    public string UserName { get; set; }

    [JsonIgnore]
    public string Password { get; set; }  // omitted from JSON
}
```

<a id="jsonpropertyattribute"></a>
### JsonPropertyAttribute — custom JSON key

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

// Serialized JSON:
// {
//     "player_id"    : 42,
//     "display_name" : "Blank",
//     "Hp"           : 100.5
// }
```

### Unity built-in types

Unity value types serialize through registered exporters — no extra attributes needed:

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

Supported Unity types: `Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Color`, `Color32`, `Rect`, `RectOffset`, `Bounds`, `AnimationCurve`.

### Type extensibility

Register your own importer / exporter for full control:

```csharp
using System;
using GameFrameX.LitJSON.Runtime;

// Convert DateTime to ISO-8601 string on write
JsonMapper.RegisterExporter<DateTime>((value, writer) =>
{
    writer.Write(value.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
});

// Parse ISO-8601 string back to DateTime on read
JsonMapper.RegisterImporter<string, DateTime>(s =>
    DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind));
```

> `UnityTypeBindings.Register()` is invoked automatically by `LitJsonCroppingHelper` on startup; you don't need to call it manually for the built-in Unity types listed above.

## Modifications vs XINCGer/LitJson4Unity

1. Added `link.xml` for stripping filter
2. Added `LitJsonCroppingHelper` anti-stripping helper
3. Batch-applied `[Preserve]` on reflection-critical members
4. Added `JsonPropertyAttribute` for custom JSON property names

## Documentation

- [GameFrameX Documentation](https://gameframex.doc.alianblank.com)
- [CHANGELOG](CHANGELOG.md)
- [LICENSE](LICENSE.md)

## Community

- QQ Group: 467608841 / 233840761

## License

Licensed under the MIT License — see [LICENSE.md](LICENSE.md).

This library serves as a sub-module for [GameFrameX](https://github.com/AlianBlank/GameFrameX).
