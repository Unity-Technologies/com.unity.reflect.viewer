# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed
* Fixed an issue where `BinarySerialization` was keeping serialized references until the next `ToBinary` call.
* Fixed an issue where `JsonSerialization` validation was not detecting open scopes at the end of a stream.

### Added
* Add `SerializeValue` API for `SerializationContext` objects for serialization re-entry.
* Add `DeserializeValue` API for `SerializationContext` objects for serialization re-entry.
* JSON serialization now supports simple json validation as an option.

### Changed
* ***Breaking change*** `IJsonAdapter` now pass a context object (`JsonSerializationContext` and `JsonDeserializationContext`). These context objects provide access to the underlying writer or serialized views. 
* ***Breaking change*** `IBinaryAdapter` now pass a context object (`BinarySerializationContext` and `BinaryDeserializationContext`). These context objects provide access to the underlying writer or reader.
  
### Fixed
* JSON serialization now properly adds escape characters to `char` value `\0`.

## [1.7.0] - 2021-02-26
### Changed
* Updated `com.unity.properties` to version `1.7.0-preview`.

## [1.6.2] - 2020-12-03
### Fixed
* Fixed a regression causing `object` fields with bool values to be serialized as a quoted string.

### Removed
* ***Breaking change*** `JsonStringBuffer` has been removed. `JsonWriter` should be used instead.

## [1.6.1] - 2020-10-22
### Fixed
* Fixed internal API compatibility with `FixedString`.

## [1.6.0] - 2020-10-07
### Changed
* Updated minimum Unity version to `2020.1`.
* Update `com.unity.burst` to version `1.3.5`.
* Update `com.unity.collections` to version `0.12.0-preview.13`.
* Update `com.unity.jobs` to version `0.5.0-preview.14`.
* Update `com.unity.properties` to version `1.6.0-preview`.
* Update `com.unity.test-framework.performance` to version `2.3.1-preview`.

### Added
* Added low level `JsonWriter` class which can be used as a forward only JSON writer.
* Added binary serialization support for `System.Guid`, `System.DateTime`, `System.TimeSpan`, `System.Version`, `System.IO.FileInfo`, `System.IO.DirectoryInfo`, `UnityEditor.GUID`, `UnityEditor.GlobalObjectId`.

## [1.5.0] - 2020-08-21
### Added
* Added unsafe constructor overload to `SerializedObjectReader` that takes `char*` and length.

### Changed
* Enabled minimal support for `NET_DOTS`. Low level tokenization and parsing is now available.
* Updated `com.unity.properties` to version `1.5.0-preview`.
  
## [1.4.3] - 2020-08-04
### Changed
* Update `com.unity.properties` to version `1.4.3-preview`.

### Added
* Added built-in support for serialize/deserialize `System.Version` in Json.

## [1.4.2] - 2020-08-03
### Changed
* Update `com.unity.properties` to version `1.4.2-preview`.

## [1.4.1] - 2020-07-31
### Changed
* Update `com.unity.properties` to version `1.4.1-preview`.

## [1.4.0] - 2020-07-30
### Changed
* Update `com.unity.properties` to version `1.4.0-preview`.

## [1.3.1] - 2020-06-11
### Changed
* Update `com.unity.properties` to version `1.3.1-preview`.

## [1.3.0] - 2020-05-13
### Added
* Added `UserData` parameter to `JsonSerializationParameters` which can be retrieved during migration in `JsonMigrationContext`.

### Changed
* Update `com.unity.properties` to version `1.3.0-preview`.

## [1.2.0] - 2020-04-03
### Changed
* Update `com.unity.properties` to version `1.2.0-preview`.

### Fixed
* Fix binary deserialization to correctly ignore properties marked with `[DontSerialize]` or `[NonSerialized]`.

### Added
* Added `Minified` option to `JsonSerializationParameters`.
* Added `Simplified` option to `JsonSerializationParameters`.

## [1.1.1] - 2020-03-20
### Changed
* Update `com.unity.properties` to version `1.1.1-preview`.

### Fixed
* Fix JSON deserialization of polymorphic array types.

## [1.1.0] - 2020-03-11
### Changed
* Update `com.unity.properties` to version `1.1.0-preview`.

### Fixed
* Fix exception thrown when encountering null `FileInfo` or `DirectoryInfo` during JSON serialization.

### Added
* Added built-in support to serialize/deserialize `LazyLoadReference` in editor. Not supported at runtime.

## [1.0.0] - 2020-03-02
### Changed
* ***Breaking change*** Complete API overhaul, see the package documentation for details.

## [0.6.3] - 2019-11-08
### Changed
* Updated `com.unity.properties` to version `0.10.3-preview`.

## [0.6.2] - 2019-11-05
### Changed
* Updated `com.unity.properties` to version `0.10.1-preview`.

### Fixed
* Reference type values set to `null` will now serialize as `null` instead of `{}`.

## [0.6.1] - 2019-10-25
### Fixed
* Fixed a major serialization regression for `UnityEngine.Object` derived objects.

## [0.6.0] - 2019-10-25
### Changed
* ***Breaking change*** `JsonSerialization.Deserialize` will now return a disposable `VisitResult` containing logs, errors and exceptions that occurred during deserialization.
* Updated `com.unity.properties` to version `0.10.0-preview`.

### Added
* Support JSON serialization of `System.DateTime` and `System.TimeSpan`.

## [0.5.1] - 2019-10-21
### Added
* Support JSON serialization of `UnityEditor.GlobalObjectId`.
* Support JSON serialization of `UnityEditor.GUID`.
* New method `DeserializeFromStream` to deserialize from stream object.

### Changed
* Updated `com.unity.properties` to version `0.9.1-preview`.
* Deserialization will now attempt to construct the destination container using `PropertyContainer.Construct` utility.
* Deserialization will now attempt to read type information field `$type` by default.

## [0.5.0] - 2019-10-07
### Changed
* Updated `com.unity.properties` to version `0.9.0-preview`.

## [0.4.1] - 2019-09-25
### Changed
* Updated `com.unity.properties` to version `0.8.1-preview`.

## [0.4.0] - 2019-09-24
### Added
* Support JSON serialization of `UnityEngine.Object`.

### Changed
* Now requires Unity 2019.3 minimum.
* Now requires `com.unity.properties` version `0.8.0-preview`.

## [0.3.1] - 2019-09-16
### Added
* Support JSON serialization of `DirectoryInfo` and `FileInfo` using string as underlying type.

## [0.3.0] - 2019-08-28
### Changed
* Updated `com.unity.properties` to version `0.7.1-preview`.
* Support JSON serialization of enums using [numeric integral types](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/integral-numeric-types) as underlying type.

## [0.2.1] - 2019-08-08
### Changed
* Support for Unity 2019.1.

## [0.2.0] - 2019-08-06
### Changed
* `JsonVisitor` virtual method `GetTypeInfo` now provides the property, container and value parameters to help with type resolution.

## [0.1.0] - 2019-07-22
* This is the first release of *Unity.Serialization*.
