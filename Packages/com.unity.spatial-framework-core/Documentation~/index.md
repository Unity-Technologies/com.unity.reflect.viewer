# About Spatial Framework Core

Use the Spatial Framework Core package to build a spatial application with various standard features.

The features currently provided in the package are listed in the [Table of Contents](TableOfContents).

## Preview package
This package is available as a preview, so it is not ready for production use. The features and documentation in this package might change before it is verified for release.

## Package contents

The following table describes the package folder structure:

>**Note**: This section is not completed yet as package contents move around

|**Location**|**Description**|
|---|---|
|*MyFolderName*|Contains &lt;describe what the folder contains&gt;.|
|*Runtime/MyFileName*|Contains &lt;describe what the file represents or implements&gt;.|

<a name="Installation"></a>

## Installation

To install this package, follow the instructions in the [Package Manager documentation](https://docs.unity3d.com/Manual/upm-ui-install.html).

Installing this package will also automatically download and add the following dependencies:
 
- InputSystem
- XRLineRenderer
- XRTools ModuleLoader
- XRTools Utils
- XR Interaction Toolkit
- TextMeshPro

## Known limitations

Spatial Framework Core version 0.1.0 includes the following known limitations:

* Everything still a work in progress


<a name="UsingSpatialFrameworkCore"></a>

# Using Spatial Framework Core

The Spatial Framework Core package contains scripts only. Various features can be integrated into another application via adding components to game objects or scripting APIs.

It is strongly reccomended to first see example scenes and prefabs configured in the Spatial Framework package that contains standard prefabs and example scenes.  

For a basic scene setup you will need the following components:
* SpatialFrameworkSceneController
* ModuleCallbacksBehaviour
* XRInteractionManager, EventSystem, XRUIInputModule
