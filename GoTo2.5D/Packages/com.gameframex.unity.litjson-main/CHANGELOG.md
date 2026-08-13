## [1.2.2](https://github.com/gameframex/com.gameframex.unity.litjson/compare/1.2.1...1.2.2) (2026-07-23)


### Bug Fixes

* **litjson:** 反序列化支持属性名大小写不敏感匹配 ([e49d3b7](https://github.com/gameframex/com.gameframex.unity.litjson/commit/e49d3b7358873e9b37070684d25c3bc35e154169)), closes [#1](https://github.com/gameframex/com.gameframex.unity.litjson/issues/1) [#1](https://github.com/gameframex/com.gameframex.unity.litjson/issues/1)

## [1.2.1](https://github.com/gameframex/com.gameframex.unity.litjson/compare/1.2.0...1.2.1) (2026-06-15)


### Bug Fixes

* **runtime:** 默认关闭 ToJson 美化输出 ([e685f34](https://github.com/gameframex/com.gameframex.unity.litjson/commit/e685f34dedd1fab5323820dfdfcee778a257f2c5))

# [1.2.0](https://github.com/gameframex/com.gameframex.unity.litjson/compare/1.1.3...1.2.0) (2026-06-15)


### Features

* **runtime:** 批量添加 [Preserve] 防裁剪标签 ([9fe0183](https://github.com/gameframex/com.gameframex.unity.litjson/commit/9fe018398c21b66a8bf4efb0f08160de595c700a))
* **runtime:** 新增 JsonIgnoreAttribute 并重命名命名空间 ([13ccea0](https://github.com/gameframex/com.gameframex.unity.litjson/commit/13ccea0f5a311d935c50fcc94e6ba36c5ec0fa53))
* **runtime:** 新增 JsonPropertyAttribute 支持自定义属性名 ([fa44547](https://github.com/gameframex/com.gameframex.unity.litjson/commit/fa445478468ef69ef0b61ee722621aba653df4b6))

# 1.0.0 (2026-06-15)


### Bug Fixes

* **ci:** 统一 .github 工作流配置 ([3a66095](https://github.com/gameframex/com.gameframex.unity.litjson/commit/3a660957472a6888a3563436214d72332beafc1f))
* JsonData.Remove() 中添加类型检查，防止 InvalidCastException ([877e810](https://github.com/gameframex/com.gameframex.unity.litjson/commit/877e810deccb8e6aeed86c84874d176d810ada7a))
* 修复 JsonData.Equals() 对 Object 和 Array 类型使用引用相等的问题 ([c108b30](https://github.com/gameframex/com.gameframex.unity.litjson/commit/c108b301aa1425be94c5f53184b9d3d904c72272))
* 修复 JsonIgnore 空引用异常——as 转换为 ICollection<Attribute> 始终返回 null ([bfc8581](https://github.com/gameframex/com.gameframex.unity.litjson/commit/bfc85811e1bb6d71583bf9b214ce7bb7c007560c))
* 修复 JsonMapper 中自定义导出/导入表的线程安全问题 ([c119d08](https://github.com/gameframex/com.gameframex.unity.litjson/commit/c119d082f9007e584779c4a4def455e14196245e))
* 修复 registerd 拼写错误，并添加 volatile 以保证线程安全 ([1ae5ec4](https://github.com/gameframex/com.gameframex.unity.litjson/commit/1ae5ec428f8155c8ebcbf1129a77f8ab797ab216))
* 修复版本tag异常的工作流 ([7b5be47](https://github.com/gameframex/com.gameframex.unity.litjson/commit/7b5be47f8ee25a8dbd6a64ce596d39834cdb09e0))
* 修复版本tag异常的工作流 ([1bb7746](https://github.com/gameframex/com.gameframex.unity.litjson/commit/1bb774609a243f8f93ca401f0317cd3f5cc92521))
* 修正 Boolean 显式转换的错误消息（此前错误复制自 Double） ([b3891d6](https://github.com/gameframex/com.gameframex.unity.litjson/commit/b3891d6b87e5eee162186376db805f7aee830dfb))
* 将 GetConvOp 中 conv_ops 的读取移入锁内，消除 TOCTOU 竞争条件 ([e514f7a](https://github.com/gameframex/com.gameframex.unity.litjson/commit/e514f7acf8920facb6ccab925d8494dc4fd65657))
* 检测 ToJson() 的重入调用，抛出异常而非死锁 ([b052f63](https://github.com/gameframex/com.gameframex.unity.litjson/commit/b052f632a6d7c208698f527a332bb417fb22eb39))
* 补全包规范文件（LICENSE/CHANGELOG/URL 字段/unity 字段） ([c6820cc](https://github.com/gameframex/com.gameframex.unity.litjson/commit/c6820cce5c79da5cce0af64043061672354f7724))


### Features

* **ci:** change ci ([a58af01](https://github.com/gameframex/com.gameframex.unity.litjson/commit/a58af0161cfe6e11783e1faa7ffd6a9bdf718a5f))
* **runtime:** 批量添加 [Preserve] 防裁剪标签 ([9fe0183](https://github.com/gameframex/com.gameframex.unity.litjson/commit/9fe018398c21b66a8bf4efb0f08160de595c700a))
* **runtime:** 新增 JsonIgnoreAttribute 并重命名命名空间 ([13ccea0](https://github.com/gameframex/com.gameframex.unity.litjson/commit/13ccea0f5a311d935c50fcc94e6ba36c5ec0fa53))
* **runtime:** 新增 JsonPropertyAttribute 支持自定义属性名 ([fa44547](https://github.com/gameframex/com.gameframex.unity.litjson/commit/fa445478468ef69ef0b61ee722621aba653df4b6))


### Performance Improvements

* JsonData.ToJson() 中复用静态 JsonWriter，减少 GC 分配 ([29518a2](https://github.com/gameframex/com.gameframex.unity.litjson/commit/29518a2ea2abcfdafab6606131ed299aaad013d1))

# Changelog

All notable changes to this project will be documented in this file. See [standard-version](https://github.com/conventional-changelog/standard-version) for commit guidelines.

## [1.1.3](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/compare/1.1.2...1.1.3) (2026-06-07)


### Bug Fixes

* 补全包规范文件（LICENSE/CHANGELOG/URL 字段/unity 字段） ([c6820cc](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/c6820cce5c79da5cce0af64043061672354f7724))

## [1.1.2](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/compare/1.1.1...1.1.2) (2026-05-30)


### Bug Fixes

* JsonData.Remove() 中添加类型检查，防止 InvalidCastException ([877e810](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/877e810deccb8e6aeed86c84874d176d810ada7a))
* 修复 JsonData.Equals() 对 Object 和 Array 类型使用引用相等的问题 ([c108b30](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/c108b301aa1425be94c5f53184b9d3d904c72272))
* 修复 JsonIgnore 空引用异常——as 转换为 ICollection<Attribute> 始终返回 null ([bfc8581](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/bfc85811e1bb6d71583bf9b214ce7bb7c007560c))
* 修复 JsonMapper 中自定义导出/导入表的线程安全问题 ([c119d08](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/c119d082f9007e584779c4a4def455e14196245e))
* 修复 registerd 拼写错误，并添加 volatile 以保证线程安全 ([1ae5ec4](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/1ae5ec428f8155c8ebcbf1129a77f8ab797ab216))
* 修正 Boolean 显式转换的错误消息（此前错误复制自 Double） ([b3891d6](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/b3891d6b87e5eee162186376db805f7aee830dfb))
* 将 GetConvOp 中 conv_ops 的读取移入锁内，消除 TOCTOU 竞争条件 ([e514f7a](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/e514f7acf8920facb6ccab925d8494dc4fd65657))
* 检测 ToJson() 的重入调用，抛出异常而非死锁 ([b052f63](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/b052f632a6d7c208698f527a332bb417fb22eb39))


### Performance Improvements

* JsonData.ToJson() 中复用静态 JsonWriter，减少 GC 分配 ([29518a2](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/29518a2ea2abcfdafab6606131ed299aaad013d1))

## [1.1.1](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/compare/1.1.0...1.1.1) (2026-05-28)


### Bug Fixes

* **ci:** 统一 .github 工作流配置 ([3a66095](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/3a660957472a6888a3563436214d72332beafc1f))

# [1.1.0](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/compare/1.0.2...1.1.0) (2025-12-23)


### Features

* **ci:** change ci ([a58af01](https://github.com/gameframex/com.gameframex.unity.xincger.litjson/commit/a58af016c1cfe6e11783e1faa7ffd6a9bdf7185f))

## [1.0.2](https://github.com/GameFrameX/com.gameframex.unity.xincger.litjson/tree/1.0.2) (2025-06-01)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.xincger.litjson/compare/1.0.1...1.0.2)

## [1.0.1](https://github.com/GameFrameX/com.gameframex.unity.xincger.litjson/tree/1.0.1) (2025-05-31)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.xincger.litjson/compare/1.0.0...1.0.1)

## [1.0.0](https://github.com/GameFrameX/com.gameframex.unity.xincger.litjson/tree/1.0.0) (2023-12-02)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.xincger.litjson/compare/f503731da3d570e3120d1bbe0b112942a1994b12...1.0.0)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
