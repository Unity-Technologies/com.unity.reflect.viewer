# Changelog
All notable changes to this package will be documented in this file.

## [Unreleased]
## Added
* Added the `InspectorElement` type to generate a UI hierarchy. This works similarly to `PropertyElement`, but allows to define a custom inspectors specifically for the root target instance.
* Added support for inlining a `UnityEngine.Object` when the field or property is tagged with the `[InlineUnityObject]` attribute.
* Added a feature that allows to display arbitrary content in an editor window or in the inspector, through the `ContentProvider` and the `SelectionUtility` types.

### Changed
* ***Breaking change*** Previous `Inspector<T>` has been renamed to `PropertyInspector<T>`.
* ***Breaking change*** `PropertyDrawer<T, TAttribure>` has been renamed to `PropertyInspector<T, TAttribure>`.
* ***Breaking change*** `Inspector<T>` can now be used to declare custom inspector on root objects, when using `InspectorElement`.
* ***Breaking change*** `AddSearchFilterProperty` and `AddSearchFilterCallback` now take a `SearchFilterOptions` to provide additional customization.

### Fixed
* Fixed `SearchHandler` to always call `OnFilter` at least once when running in `async` mode regardless of results.
* In Unity versions prior to `2020.2`, Enums using an underlying type of `long` will now be skipped when generating the UI hierarchy, since they can't be represented correctly.

## [1.7.0] - 2021-02-26
### Changed
* Updated `com.unity.properties` to version `1.7.0-preview`.
* Updated `com.unity.serialization` to version `1.7.0-preview`.

### Added
* Added `GlobalStringComparison` option to `SearchElement`.

### Fixed
* Reduced allocations when executing searches using `SearchElement`.
* In Unity versions prior to `2020.2`, Enums using an underlying type of `long` will now be skipped when generating the UI hierarchy, since they can't be represented correctly.

## [1.6.3] - 2021-01-15
### Fixed
* Fixed `SearchElement` treating double quotes differently when the `com.unity.quicksearch` package is not installed.

## [1.6.2] - 2020-11-04
### Fixed
* Added support for `com.unity.quicksearch` version `3.0.0-preview.2`.

## [1.6.1] - 2020-10-28
### Fixed
* Fixed a crash when using the `SearchElement` "Add filter" button on OSX.
* Updated `com.unity.serialization` to version `1.6.1-preview`.
 
## [1.6.0] - 2020-10-07
### Changed
* Updated minimum Unity version to `2020.1`.
* Updated minimum supported version of `com.unity.quicksearch` to `2.1.0-preview.4`.
* Updated `com.unity.properties` to version `1.6.0-preview`.
* Updated `com.unity.serialization` to version `1.6.0-preview`.

### Fixed
* Fixed an issue where resetting the `Inspector<T>.Target` would throw an exception when nesting `PropertyElement`.

## [1.5.0] - 2020-08-21
### Changed
* SearchElement: `PropertiesSearchBackend` now ignores tokens containing filters. Filter are only supported by QuickSearch backend.
* SearchElement: `QuickSearchBackend` now ignores unknown filters.
* Updated `com.unity.properties` to version `1.5.0-preview`.
* Updated `com.unity.serialization` to version `1.5.0-preview`.

### Fixed
* SearchElement: Fixed exception thrown when getting `Tokens` property on a query built from an empty, whitespaces only or null SearchString.
* When using `StylingUtility.AlignInspectorLabelWidth`, the labels will be clipped instead of being shown in the background.

## [1.4.3] - 2020-08-04
### Added
* Added a default inspector for the `System.Version` type.
* Added `Inspector<T>.IsReadOnly` to indicate if the target of a custom inspector is read only.

### Changed
* Update `com.unity.properties` to version `1.4.3-preview`.
* Update `com.unity.serialization` to version `1.4.3-preview`.

## [1.4.2] - 2020-08-04
### Fixed
* Fixed issues where removing an element from an array would not preserve subsequent elements.

### Added
* Added `StylingUtility.AlignInspectorLabelWidth` helper method to force labels to be dynamically computed so that they would align properly.

### Changed
* Update `com.unity.properties` to version `1.4.2-preview`.
* Update `com.unity.serialization` to version `1.4.2-preview`.

## [1.4.0] - 2020-07-30
### Added
* Added `SearchElement`, a reusable control which can use property bindings for search data fields and filtering. The functionality is extended when the `com.unity.quicksearch` packages is installed.
* Added `com.unity.modules.uielements` as a dependency

## [1.3.1] - 2020-06-11
### Fixed
* Fixed custom inspectors for enum types not being considered. 
* Fixed `ObjectField` applying the `Texture2D` value as a background image, when `objectType` is set to `typeof(Texture2D)`.
* Fixed constant repainting when display a list.
* Fixed `InspectionContext` not being properly propagated with nested custom inspectors. 

### Changed
* Updated `com.unity.properties` to version `1.3.1-preview`.
* Updated `com.unity.serialization` to version `1.3.1-preview`.
  
## [1.3.0] - 2020-05-13
### Added
* Added support for using "." in `uxml` files to refer to the value being inspected in a custom inspector.
* Added support for automatically nesting `PropertyElement` using `bindingPath`.
* Added support for passing an inspection context to a ' PropertyElement'.
* Added overloads to `Inspector<TValue>.DoDefaultGUI[...]` to use the default drawer for a given property path, index or key.
* Added support for creating array instances. 

### Changed
* Updated `com.unity.properties` to version `1.3.0-preview`.
* Updated `com.unity.serialization` to version `1.3.0-preview`.

### Fixed
* Fixed specifying nested paths (i.e.: binding-path="path.to.field") in custom inspectors.
* Fixed binding to `UnityEngine.UIElements.Label` requiring a type converter to be registered.
* Fixed single line list items not shrinking properly.
* Fixed stack overflow issues that could happen when calling `DoDefaultGui()`.
* Fixed displaying type names for nested generic types.
* Fixed array fields not being editable.

## [1.2.0] - 2020-04-03
### Changed
* Updated `com.unity.properties` to version `1.2.0-preview`.
* Updated `com.unity.serialization` to version `1.2.0-preview`.

## [1.1.1] - 2020-03-20
### Fixed
* Fix `AttributeFilter` incorrectly being called on the internal property wrapper.

### Changed
* Updated `com.unity.properties` to version `1.1.1-preview`.
* Updated `com.unity.serialization` to version `1.1.1-preview`.

## [1.1.0] - 2020-03-11
### Fixed
* Fixed background color not being used when adding new collection items.
* Fixed readonly arrays being resizable from the inspector.

### Changed
* Updated `com.unity.properties` to version `1.1.0-preview`.
* Updated `com.unity.serialization` to version `1.1.0-preview`.

### Added
* Added the `InspectorAttribute`, allowing to put property attributes on both fields and properties.
* Added the `DelayedValueAttribute`, which works similarly to `UnityEngine.DelayedAttribute`, but can work with properties.
* Added the `DisplayNameAttribute`, which works similarly to `UnityEngine.InspectorNameAttribute`, but can work with properties.
* Added the `MinValueAttribute`, which works similarly to `UnityEngine.MinAttribute`, but can work with properties.
* Added built-in inspector for `LazyLoadReference`.

## [1.0.0] - 2020-03-02
### Changed
* ***Breaking change*** Complete API overhaul, see the package documentation for details.
