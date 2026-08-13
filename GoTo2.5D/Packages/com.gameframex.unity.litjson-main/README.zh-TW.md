<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X LitJson

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

獨立遊戲開發的一站式解決方案 · 為獨立開發者的夢想賦能

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · [特性](#特性) · QQ Group: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 專案簡介

為 Unity 打造的改良版 **LitJson** 函式庫，基於 [XINCGer/LitJson4Unity](https://github.com/XINCGer/LitJson4Unity) 重新封裝，並針對 GameFrameX 生態系進行擴充。

它在 [原始 LitJson](https://github.com/LitJSON/litjson) 的基礎上，新增了對 `float` 型別、Unity 內建實值型別、屬性驅動的序列化控制以及 IL2CPP 裁剪保護的第一級支援 — 讓它在啟用託管程式碼裁剪的 Unity 專案中也能正式上線。

> 命名空間：`GameFrameX.LitJSON.Runtime`

## 特性

### 在原生 LitJson 基礎上的增強

- **`float` 型別支援** — `float` 的第一級序列化（原生版本僅支援 `double`）
- **Unity 內建型別** — `Vector2` / `Vector3` / `Vector4`、`Quaternion`、`Color` / `Color32`、`Rect` / `RectOffset`、`Bounds`、`AnimationCurve`
- **[`JsonIgnoreAttribute`](#jsonignoreattribute)** — 序列化時跳過某個欄位或屬性
- **[`JsonPropertyAttribute`](#jsonpropertyattribute)** — 將 C# 成員對應到自訂的 JSON 鍵名
- **預設格式化輸出** — `JsonMapper.ToJson` 預設產生美化排版的 JSON；傳入 `prettyPrint: false` 可取得緊湊輸出
- **型別 importer / exporter 擴充性** — 透過 `JsonMapper.RegisterExporter<T>` 與 `RegisterImporter<TJson, TValue>` 註冊自訂轉換

### IL2CPP / 託管程式碼裁剪保護

隨附三層互補的安全機制，確保此函式庫能在 Unity 發行版本中的託管程式碼裁剪下存活：

- `link.xml` — 宣告式型別保留
- `LitJsonCroppingHelper` — 執行期反裁剪輔助器，啟動時透過反射觸碰關鍵型別
- `[Preserve]` 屬性 — 批次套用於對反射敏感的成員（建構函式、屬性 getter、exporter），讓 IL2CPP 保留它們

## 快速開始

### 安裝

請選擇下列其中一種方式。

#### 方式 1 — Scoped Registry（推薦）

編輯 Unity 專案的 `Packages/manifest.json`：

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

只有名稱以 `com.gameframex` 開頭的套件會透過此 registry 解析。

#### 方式 2 — 直接在 manifest.json 中使用 Git URL

```json
{
  "com.gameframex.unity.litjson": "https://github.com/gameframex/com.gameframex.unity.litjson.git"
}
```

#### 方式 3 — Unity Package Manager → Add package from git URL

```
https://github.com/gameframex/com.gameframex.unity.litjson.git
```

#### 方式 4 — 手動 clone

將此儲存庫 clone 到 Unity 專案的 `Packages` 目錄下，Unity 會自動載入它。

### 基本用法

```csharp
using GameFrameX.LitJSON.Runtime;

class PlayerData
{
    public int Id { get; set; }

    public string Name { get; set; }

    public float Hp { get; set; }
}

// 序列化（預設美化排版）
var player = new PlayerData { Id = 42, Name = "Blank", Hp = 100.5f };
string json = JsonMapper.ToJson(player);
/*
{
    "Id"   : 42,
    "Name" : "Blank",
    "Hp"   : 100.5
}
*/

// 緊湊輸出
string compact = JsonMapper.ToJson(player, false);  // {"Id":42,"Name":"Blank","Hp":100.5}

// 反序列化
PlayerData restored = JsonMapper.ToObject<PlayerData>(json);
```

<a id="jsonignoreattribute"></a>
### JsonIgnoreAttribute — 跳過成員

```csharp
using GameFrameX.LitJSON.Runtime;

class Account
{
    public string UserName { get; set; }

    [JsonIgnore]
    public string Password { get; set; }  // 序列化時被跳過
}
```

<a id="jsonpropertyattribute"></a>
### JsonPropertyAttribute — 自訂 JSON 鍵名

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

// 序列化後的 JSON：
// {
//     "player_id"    : 42,
//     "display_name" : "Blank",
//     "Hp"           : 100.5
// }
```

### Unity 內建型別

Unity 實值型別透過已註冊的 exporter 進行序列化 — 不需要額外的屬性：

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

支援的 Unity 型別：`Vector2`、`Vector3`、`Vector4`、`Quaternion`、`Color`、`Color32`、`Rect`、`RectOffset`、`Bounds`、`AnimationCurve`。

### 型別擴充

註冊自己的 importer / exporter 以取得完全控制：

```csharp
using System;
using GameFrameX.LitJSON.Runtime;

// 寫入時將 DateTime 轉為 ISO-8601 字串
JsonMapper.RegisterExporter<DateTime>((value, writer) =>
{
    writer.Write(value.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
});

// 讀取時將 ISO-8601 字串剖析回 DateTime
JsonMapper.RegisterImporter<string, DateTime>(s =>
    DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind));
```

> `UnityTypeBindings.Register()` 會由 `LitJsonCroppingHelper` 在啟動時自動呼叫；對上述內建 Unity 型別，您不需要手動呼叫。

## 相對 XINCGer/LitJson4Unity 的改動

1. 新增 `link.xml` 作為裁剪過濾器
2. 新增 `LitJsonCroppingHelper` 反裁剪輔助器
3. 在對反射敏感的成員上批次套用 `[Preserve]`
4. 新增 `JsonPropertyAttribute` 以自訂 JSON 屬性名稱

## 文件

- [GameFrameX Documentation](https://gameframex.doc.alianblank.com)
- [CHANGELOG](CHANGELOG.md)
- [LICENSE](LICENSE.md)

## 社群

- QQ Group: 467608841 / 233840761

## 授權條款

採用 MIT License 授權 — 詳見 [LICENSE.md](LICENSE.md)。

此函式庫為 [GameFrameX](https://github.com/AlianBlank/GameFrameX) 的子模組。
