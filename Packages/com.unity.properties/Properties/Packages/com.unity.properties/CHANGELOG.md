# Changelog
All notable changes to this package will be documented in this file.

## [Unreleased]
### Added
* Added `PropertyBag.GetPropertyBag(Type)` and `PropertyBag.GetPropertyBag<T>()` API methods.
* Added all visitor interfaces to the public API (`IPropertyBagVisitor`, `IPropertyVisitor` etc). These can be used to write lower level and more specialized visitors.
* Added the `InlineUnityObjectAttribute` to inform visitation that a `UnityEngine.Object` field or property should be considered as a normal type rather than a reference.
* Added `PropertyBag.CreateInstance` API methods which allow type construction through an explicit delegate, property bag implementation or activator.
* Added `Reflection.PropertyBagUtility.RequestPropertyBagGeneration` API methods to generate a property bag before any visitation.

### Changed
* ***Breaking change*** Visitation adapters (`IExclude` and `IVisit`) now pass a context object (respectively `ExcludeContext` and `VisitContext`) instead of the `IProperty`.
* ***Breaking change*** `Unity.Properties.VisitStatus` has been removed. Visitation adapters now requires an explicit call to `ContinueVisitation` (equivalent of `VisitStatus.Unhandled`) or `ContinueVisitationWithoutAdapters` (equivalent of `VisitStatus.Handled`) in order continue visitation. 
* ***Breaking change*** `PropertyContainer.Visit` has been renamed to `PropertyContainer.Accept` and the parameter order has changed.
* ***Breaking change*** `TypeConstruction` has been moved from `Unity.Properties.Editor` to `Unity.Properties` and is now available at runtime.
* ***Breaking change*** `GetAllConstructableTypes` has been moved from `TypeConstruction` to `TypeUtility` and remains editor only.
* ***Breaking change*** `CanBeConstructedFromDerivedType` has been moved from `TypeConstruction` to `TypeUtility` and remains editor only.
* ***Breaking change*** Renamed various `PropertyBag.Register` methods to provide overloads for common collection types.
* ***Breaking change*** `IPropertyBag<T>.GetProperties` now returns a `PropertyCollection<T>` instead of an `IEnumerable<IProperty<T>>`. The `PropertyCollection<T>` struct can be used to enumerate properties with no allocations in common cases.
* ***Breaking change*** `TypeConversion.Convert` and `TypeConversion.TryConvert` methods must now pass the source parameter as a `ref`.
* ***Breaking change*** `TypeConversion.ConvertDelegate` now passes the source parameters by `ref`.

### Removed
* ***Breaking change*** `IProperty.Visit<TValue>(PropertyVisitor, TValue)` has been removed. Use `PropertyContainer.Accept(IPropertyBagVisitor, TContainer)` instead.

### Fixed
* Fixed `PropertyContainer.IsPathValid` throwing an exception when a `null` value is visited on the given path.

## [1.7.0] - 2021-02-26
### Added
* Added overloads to `TypeConversion.Convert` and `TypeConversion.TryConvert` to pass the source as a `ref` to avoid creating copies during conversion.

## [1.6.0] - 2020-10-07
### Changed
* Updated minimum Unity version to `2020.1`.
* Updated `com.unity.test-framework.performance` to version `2.3.1-preview`.

### Fixed
* CodeGen: Fixed cross assembly base type references.

## [1.5.0] - 2020-08-21
### Changed
* Enabled minimal support for `NET_DOTS`. `Property`, `PropertyBag` and `PropertyContainer.Visit` are now available.

## [1.4.3] - 2020-08-04
### Fixed
* Fixed error logged during visitation when nested visitation contains the same property path.
* CodeGen: Fixed exception thrown when trying to generate property bags for anonymous types.

## [1.4.2] - 2020-08-04
### Fixed
* Fixed issues where a test would fail when using IL2CPP.

### Added
* Added a built-in property bag for `System.Version`.

## [1.4.1] - 2020-07-31
### Fixed
* Fixed issues where removing an element from an array would not preserve subsequent elements.

## [1.4.0] - 2020-07-30
### Fixed
* Fixed error logged during visitation when nested visitation contains the same property path.
* Fixed `PropertyVisitor.VisitCollection` not being invoked for `null` collection types.

### Changed
* Updated `com.unity.nuget.mono-cecil` to version `0.1.6-preview.2`.

## [1.3.1] - 2020-06-11
### Fixed
* `TypeUtility.GetRootType` will now return `null` when provided an interface type.
* Fixed exception thrown when trying to generate property bags for multidimensional array types. Multidimensional arrays are not supported.
* CodeGen: Fixed exception thrown when trying to generate property bags for multidimensional array types. Multidimensional arrays are not supported.
* CodeGen: Fixed cross assembly type references for C# properties.
* CodeGen: Fixed issue when initializing attributes for private members of base types.

## [1.3.0] - 2020-05-13
### Added
* Added a utility class around `System.Type`.

### Fixed
* `PropertyContainer.SetValue` will not throw `AccessViolationException` when trying to set a `read-only` property.
* CodeGen: Fixed invalid IL produced by open generic root types.

## [1.2.0] - 2020-04-03
### Fixed
* CodeGen: Fixed arrays of arrays.
* CodeGen: Fixed types with generic bases.

### Added
* CodeGen: Add `GeneratePropertyBagsInEditorAttribute` which can be used to enable Editor time codegen per assembly.

## [1.1.1] - 2020-03-20
### Fixed
* Fixed an issue where the `PropertyVisitor.IsExcluded` override was only being called when adapters were registered.

## [1.1.0] - 2020-03-11
### Fixed
* Fixed list elements incorrectly being considered as `readonly` if the list was `readonly`.
* Fixed codegen not correctly registering property bags for array types.

## [1.0.0] - 2020-03-02
### Changed
* ***Breaking change*** Complete API overhaul, see the package documentation for details.

## [0.10.3] - 2019-11-08
### Fixed
* AOT Fix: Allows for registering container types with the internal generic virtual calls in Properties

## [0.10.2] - 2019-11-07
### Changed
* Missing type identifier key meta data in `PropertyContainer.Construct` is no longer considered an error, but is still reported in result logs.

### Fixed
* Calling `PropertyContainer.Transfer` when destination container have properties without setters will no longer throws.

## [0.10.1] - 2019-10-29
### Added
* Added a helper class to drive code generation in order to support AOT platforms.

## [0.10.0] - 2019-10-25
### Changed
* ***Breaking change*** `PropertyContainer.Construct` and `PropertyContainer.Transfer` will now return a disposable `VisitResult` containing logs, errors and exceptions that occurred during visitation.

## [0.9.2] - 2019-10-21
### Added
* Added support for renamed fields using `UnityEngine.Serialization.FormerlySerializedAsAttribute` in the transfer visitor.

## [0.9.1] - 2019-10-18
### Added
* Added `PropertyContainer.Construct` API call. This method can be used to initialize a tree using the default constructor for any uninitialized types.
* Added support for instantiating `UnityEngine.ScriptableObject` derived types using the `TypeConstruction` utility.

### Changed
* `PropertyContainer.Transfer` will now visit the source instead of the destination when transfering.

## [0.9.0] - 2019-10-06
### Added
* Added `TypeConstruction.TryConstruct[...]` variants for instantiating types without throwing exceptions.
* Support for property drawers.

### Fixed
* Attributes on a collection will now be correctly propagated to its elements.

### Changed
* Connectors are now registered on the `BaseField<T>` directly instead of the explicit types (i.e. `IntegerField`), which will help with user defined types.
* Added `PropertyContainer.Visit` overload with `ref TVisitor`.
* ***Breaking change*** Changed all `IPropertyBag{T}.Accept` methods to pass the `TVisitor` by ref.

## [0.8.1] - 2019-09-25
### Fixed
* Public fields and properties from base class will now again be reflected correctly.

## [0.8.0] - 2019-09-24
### Added
* Added a `TypeConstruction` utility to allow the creation of new instances.
* Minimal unity version has been updated to 2019.3.
* Added `PropertyElement` to help with generic, property-backed UI inspectors

### Fixed
* Fixed all `PropertyContainer.Try[...]` methods to not throw exceptions when visiting nested types.
* Fixed property bag reflection duplicates when base class contains an internal field or property.

## [0.7.2] - 2019-09-12
### Changed
* Exposed a default way to manually visit collection items, through `VisitCollectionElementCallback<TContainer>`

### Added
* Added `PropertyContainer.TryGetValueAtPath` and `PropertyContainer.TrySetValueAtPath`, which will try to set a value for a given `PropertyPath`.
* Added `PropertyContainer.TryGetCountAtPath` and `PropertyContainer.TrySetCountAtPath`, which will try to set the count of a collection for a given `PropertyPath`.
* Added `PropertyContainer.VisitAtPath` and `PropertyContainer.TryVisitAtPath`, which will do a partial visitation for a given `PropertyPath`.

## [0.7.1] - 2019-08-29
### Fixed
* Narrowing conversions between supported enum types will not throw an `InvalidCastException` anymore.

## [0.7.0] - 2019-08-23
### Fixed
* Conversion to all supported underlying type of enums should now be supported.
* Type conversion should now work on derived types.

### Added
* Added `PropertyContainer.GetValueAtPath` and `PropertyContainer.SetValueAtPath`, which will set a value for a given `PropertyPath`.
* Added construction of a `PropertyPath` from a string (i.e. `Path.To.The.List[1].Value`).

### Changed
* ***Breaking change*** `IPropertyGetter` and `ICollectionPropertyGetter` are now passed by ref during visitation.

## [0.6.4] - 2019-08-15

### Fixed
* Fixed property bag reflection for base class with private properties.
* Disabled generation of properties for reflected pointer fields in order to avoid casting errors.

## [0.6.3] - 2019-08-06

### Fixed
* Fixed `System.Guid` properties `IsContainer` value to return `false`.
* Fixed property bag reflection for base class with private fields.
* Fixed property bag reflection for private properties.

## [0.6.2] - 2019-07-29

### Fixed
* Fixed property bag resolution for boxed and interface types.

## [0.6.1] - 2019-07-25

### Fixed
* Fixed the reflection property generator to correclty handle `IList<T>`, `List<T>` and `T[]` collection types.
* Fixed `ArgumentNullException` when visiting a null container.

## [0.6.0] - 2019-07-19

### Added
* Added `[Property]` attribute which can be used on fields or C# properties. The attribute will force the reflection generator to include the member.

### Changed
* `PropertyBagResolver.RegisterProvider` has been removed and replaced with access to a static `ReflectedPropertyBagProvider`.
* `Unity.Properties.Reflection` assembly has been removed and merged with `Unity.Properties`.

### Fixed
* `TypeConverter` no longer warns if the source and destination types are the same.
* TypeConversion of enum types will now convert based on the value and not the index.
* PropertyContainer.Transfer now ensures destination type is a reference type when not passed by ref.
* Fix generated properties for `List<string>` incorrectly treating strings as container types.
* `UnmanagedProperty` can now be generated for `char` types during reflection.

## [0.5.0] - 2019-04-29

### Changed
* Complete refactor of the Properties package.

