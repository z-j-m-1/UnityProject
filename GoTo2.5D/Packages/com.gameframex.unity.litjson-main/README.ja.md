<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X LitJson

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

インディーゲーム開発のためのオールインワンソリューション · インディーデベロッパーの夢を後押しします

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [クイックスタート](#クイックスタート) · [特徴](#特徴) · QQ Group: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md)

</div>

## プロジェクト概要

[XINCGer/LitJson4Unity](https://github.com/XINCGer/LitJson4Unity) を再パッケージ化し、GameFrameX エコシステム向けに拡張した、Unity 向けの改良版 **LitJson** ライブラリです。

[オリジナルの LitJson](https://github.com/LitJSON/litjson) をベースにしつつ、`float` 型のファーストクラスサポート、Unity 組み込みの値型、属性によるシリアライズ制御、IL2CPP のコード削除対策を追加しています。マネージドコード削除が有効な Unity プロジェクトでも、そのままプロダクション環境で利用できます。

> 名前空間: `GameFrameX.LitJSON.Runtime`

## 特徴

### オリジナル LitJson からの拡張

- **`float` 型サポート** — `float` のファーストクラスシリアライズ（オリジナルは `double` のみ対応）
- **Unity 組み込み型** — `Vector2` / `Vector3` / `Vector4`、`Quaternion`、`Color` / `Color32`、`Rect` / `RectOffset`、`Bounds`、`AnimationCurve`
- **[`JsonIgnoreAttribute`](#jsonignoreattribute)** — シリアライズ時にフィールドやプロパティをスキップ
- **[`JsonPropertyAttribute`](#jsonpropertyattribute)** — C# のメンバーを任意の JSON キーにマッピング
- **デフォルトで整形出力** — `JsonMapper.ToJson` はデフォルトで整形済み JSON を生成します。コンパクトな出力が必要な場合は `prettyPrint: false` を渡してください
- **型インポーター / エクスポーターの拡張性** — `JsonMapper.RegisterExporter<T>` や `RegisterImporter<TJson, TValue>` でカスタム変換を登録可能

### IL2CPP / マネージドコード削除対策

Unity のマネージドコード削除（managed code stripping）に対抗するため、3 つの相補的な保護機能を同梱しています。リリースビルドでも本ライブラリが保持されます。

- `link.xml` — 宣言的な型保持
- `LitJsonCroppingHelper` — 起動時にリフレクションで重要な型を参照し、削除を防ぐランタイムヘルパー
- `[Preserve]` 属性 — リフレクションに依存するメンバー（コンストラクタ、プロパティの getter、エクスポーターなど）に一括付与し、IL2CPP が保持するように指定

## クイックスタート

### インストール

以下のいずれかの方法を選んでください。

#### 方法 1 — スコープドレジストリ（推奨）

Unity プロジェクトの `Packages/manifest.json` を編集します。

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

このレジストリ経由で解決されるのは、名前が `com.gameframex` で始まるパッケージだけです。

#### 方法 2 — manifest.json に直接 Git URL を指定

```json
{
  "com.gameframex.unity.litjson": "https://github.com/gameframex/com.gameframex.unity.litjson.git"
}
```

#### 方法 3 — Unity Package Manager → Add package from git URL

```
https://github.com/gameframex/com.gameframex.unity.litjson.git
```

#### 方法 4 — 手動クローン

このリポジトリを Unity プロジェクトの `Packages` ディレクトリにクローンしてください。Unity が自動的に認識します。

### 基本的な使い方

```csharp
using GameFrameX.LitJSON.Runtime;

class PlayerData
{
    public int Id { get; set; }

    public string Name { get; set; }

    public float Hp { get; set; }
}

// シリアライズ（デフォルトで整形出力）
var player = new PlayerData { Id = 42, Name = "Blank", Hp = 100.5f };
string json = JsonMapper.ToJson(player);
/*
{
    "Id"   : 42,
    "Name" : "Blank",
    "Hp"   : 100.5
}
*/

// コンパクト出力
string compact = JsonMapper.ToJson(player, false);  // {"Id":42,"Name":"Blank","Hp":100.5}

// デシリアライズ
PlayerData restored = JsonMapper.ToObject<PlayerData>(json);
```

<a id="jsonignoreattribute"></a>
### JsonIgnoreAttribute — メンバーをスキップ

```csharp
using GameFrameX.LitJSON.Runtime;

class Account
{
    public string UserName { get; set; }

    [JsonIgnore]
    public string Password { get; set; }  // JSON 出力に含めない
}
```

<a id="jsonpropertyattribute"></a>
### JsonPropertyAttribute — JSON キー名のカスタマイズ

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

// シリアライズ結果:
// {
//     "player_id"    : 42,
//     "display_name" : "Blank",
//     "Hp"           : 100.5
// }
```

### Unity 組み込み型

Unity の値型は登録済みのエクスポーター経由でシリアライズされるため、追加の属性は不要です。

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

対応する Unity 型: `Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Color`, `Color32`, `Rect`, `RectOffset`, `Bounds`, `AnimationCurve`。

### 型の拡張

独自のインポーター / エクスポーターを登録すれば、完全に制御できます。

```csharp
using System;
using GameFrameX.LitJSON.Runtime;

// 書き込み時に DateTime を ISO-8601 文字列に変換
JsonMapper.RegisterExporter<DateTime>((value, writer) =>
{
    writer.Write(value.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
});

// 読み取り時に ISO-8601 文字列を DateTime に戻す
JsonMapper.RegisterImporter<string, DateTime>(s =>
    DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind));
```

> `UnityTypeBindings.Register()` は起動時に `LitJsonCroppingHelper` から自動的に呼び出されるため、上記の組み込み Unity 型に対して手動で呼び出す必要はありません。

## XINCGer/LitJson4Unity からの変更点

1. コード削除フィルター用の `link.xml` を追加
2. 削除対策ヘルパー `LitJsonCroppingHelper` を追加
3. リフレクションに依存する重要なメンバーに `[Preserve]` を一括付与
4. カスタム JSON プロパティ名のための `JsonPropertyAttribute` を追加

## Documentation

- [GameFrameX Documentation](https://gameframex.doc.alianblank.com)
- [CHANGELOG](CHANGELOG.md)
- [LICENSE](LICENSE.md)

## コミュニティ

- QQ Group: 467608841 / 233840761

## ライセンス

MIT License の下でライセンスされています。詳しくは [LICENSE.md](LICENSE.md) を参照してください。

本ライブラリは [GameFrameX](https://github.com/AlianBlank/GameFrameX) のサブモジュールとして提供されています。
