<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X LitJson

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.litjson)](https://github.com/GameFrameX/com.gameframex.unity.litjson/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

인디 게임 개발을 위한 All-in-One 솔루션 · 인디 개발자의 꿈을 지원합니다

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [빠른 시작](#빠른-시작) · [기능](#기능) · QQ Group: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>

## 프로젝트 개요

Unity용으로 개선된 **LitJson** 라이브러리로, [XINCGer/LitJson4Unity](https://github.com/XINCGer/LitJson4Unity)를 기반으로 재패키징하고 GameFrameX 생태계에 맞게 확장했습니다.

[원본 LitJson](https://github.com/LitJSON/litjson)을 바탕으로 `float` 타입에 대한 일급 지원, Unity 빌트인 값 타입, 어트리뷰트 기반 직렬화 제어, IL2CPP 스트리핑 보호를 추가하여 관리 코드 스트리핑이 활성화된 상태로 출시되는 Unity 프로젝트에서도 프로덕션 환경에 바로 사용할 수 있습니다.

> 네임스페이스: `GameFrameX.LitJSON.Runtime`

## 기능

### 원본 LitJson 대비 확장 기능

- **`float` 타입 지원** — `float`에 대한 일급 직렬화 지원 (원본은 `double`만 처리)
- **Unity 빌트인 타입** — `Vector2` / `Vector3` / `Vector4`, `Quaternion`, `Color` / `Color32`, `Rect` / `RectOffset`, `Bounds`, `AnimationCurve`
- **[`JsonIgnoreAttribute`](#jsonignoreattribute)** — 직렬화 시 필드나 프로퍼티를 건너뜁니다
- **[`JsonPropertyAttribute`](#jsonpropertyattribute)** — C# 멤버를 커스텀 JSON 키에 매핑합니다
- **기본 포맷팅 출력** — `JsonMapper.ToJson`은 기본적으로 보기 좋게 출력된 JSON을 생성합니다. `prettyPrint: false`를 전달하면 압축된 출력을 얻을 수 있습니다
- **타입 임포터 / 익스포터 확장성** — `JsonMapper.RegisterExporter<T>`와 `RegisterImporter<TJson, TValue>`로 커스텀 변환을 등록할 수 있습니다

### IL2CPP / 관리 코드 스트리핑 보호

이 라이브러리는 Unity의 관리 코드 스트리핑이 활성화된 릴리스 빌드에서도 살아남을 수 있도록 세 가지 상호 보완적인 보호 장치를 제공합니다:

- `link.xml` — 선언적 타입 보존
- `LitJsonCroppingHelper` — 시작 시 리플렉션으로 핵심 타입을 참조하여 스트리핑을 방지하는 런타임 헬퍼
- `[Preserve]` 어트리뷰트 — 리플렉션에 민감한 멤버(생성자, 프로퍼티 getter, 익스포터 등)에 일괄 적용되어 IL2CPP가 이를 유지하도록 합니다

## 빠른 시작

### 설치

아래 방법 중 하나를 선택하세요.

#### 방법 1 — 스코프드 레지스트리(권장)

Unity 프로젝트의 `Packages/manifest.json`을 수정하세요:

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

이 레지스트리를 통해 `com.gameframex`로 시작하는 이름의 패키지만 해석됩니다.

#### 방법 2 — manifest.json에 직접 Git URL 사용

```json
{
  "com.gameframex.unity.litjson": "https://github.com/gameframex/com.gameframex.unity.litjson.git"
}
```

#### 방법 3 — Unity Package Manager → Add package from git URL

```
https://github.com/gameframex/com.gameframex.unity.litjson.git
```

#### 방법 4 — 수동 클론

이 저장소를 Unity 프로젝트의 `Packages` 디렉터리에 클론하면 Unity가 자동으로 인식합니다.

### 기본 사용법

```csharp
using GameFrameX.LitJSON.Runtime;

class PlayerData
{
    public int Id { get; set; }

    public string Name { get; set; }

    public float Hp { get; set; }
}

// 직렬화 (기본적으로 보기 좋게 출력됨)
var player = new PlayerData { Id = 42, Name = "Blank", Hp = 100.5f };
string json = JsonMapper.ToJson(player);
/*
{
    "Id"   : 42,
    "Name" : "Blank",
    "Hp"   : 100.5
}
*/

// 압축된 출력
string compact = JsonMapper.ToJson(player, false);  // {"Id":42,"Name":"Blank","Hp":100.5}

// 역직렬화
PlayerData restored = JsonMapper.ToObject<PlayerData>(json);
```

<a id="jsonignoreattribute"></a>
### JsonIgnoreAttribute — 멤버 건너뛰기

```csharp
using GameFrameX.LitJSON.Runtime;

class Account
{
    public string UserName { get; set; }

    [JsonIgnore]
    public string Password { get; set; }  // JSON 출력에서 제외
}
```

<a id="jsonpropertyattribute"></a>
### JsonPropertyAttribute — 커스텀 JSON 키 이름

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

// 직렬화된 JSON:
// {
//     "player_id"    : 42,
//     "display_name" : "Blank",
//     "Hp"           : 100.5
// }
```

### Unity 빌트인 타입

Unity 값 타입은 등록된 익스포터를 통해 직렬화되므로 추가 어트리뷰트가 필요 없습니다:

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

지원되는 Unity 타입: `Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Color`, `Color32`, `Rect`, `RectOffset`, `Bounds`, `AnimationCurve`.

### 타입 확장

완전한 제어를 위해 자신만의 임포터 / 익스포터를 등록할 수 있습니다:

```csharp
using System;
using GameFrameX.LitJSON.Runtime;

// 쓰기 시 DateTime을 ISO-8601 문자열로 변환
JsonMapper.RegisterExporter<DateTime>((value, writer) =>
{
    writer.Write(value.ToString("o", System.Globalization.CultureInfo.InvariantCulture));
});

// 읽기 시 ISO-8601 문자열을 다시 DateTime으로 파싱
JsonMapper.RegisterImporter<string, DateTime>(s =>
    DateTime.Parse(s, null, System.Globalization.DateTimeStyles.RoundtripKind));
```

> `UnityTypeBindings.Register()`은 시작 시 `LitJsonCroppingHelper`에 의해 자동으로 호출되므로 위에 나열된 빌트인 Unity 타입에 대해서는 수동으로 호출할 필요가 없습니다.

## XINCGer/LitJson4Unity 대비 변경 사항

1. 스트리핑 필터용 `link.xml` 추가
2. `LitJsonCroppingHelper` 안티 스트리핑 헬퍼 추가
3. 리플렉션에 중요한 멤버에 `[Preserve]` 일괄 적용
4. 커스텀 JSON 프로퍼티 이름을 위한 `JsonPropertyAttribute` 추가

## 문서

- [GameFrameX 문서](https://gameframex.doc.alianblank.com)
- [CHANGELOG](CHANGELOG.md)
- [LICENSE](LICENSE.md)

## 커뮤니티

- QQ Group: 467608841 / 233840761

## 라이선스

MIT 라이선스로 배포됩니다 — 자세한 내용은 [LICENSE.md](LICENSE.md)를 참조하세요.

이 라이브러리는 [GameFrameX](https://github.com/AlianBlank/GameFrameX)의 서브 모듈 역할을 합니다.
