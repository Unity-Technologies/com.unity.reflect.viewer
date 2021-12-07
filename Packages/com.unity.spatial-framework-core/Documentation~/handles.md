# Handles

Objects that can be grabbed and moved via direct or ray interaction.

## Base Handle

The base class for all handles is a base implementation of an XR Interactable.

The interaction events are converted into handle events that allow specific drag actions. 

## Handle Settings
The **HandleSettings** asset located in `Assets/SpatialFramework/Settings/Resources/HandleSettings` contains various settings used by the handles.
Check the tooltips on each property to see what they are for, but can generally kept as the default values.
 
## Types of Handles

* Linear
* Radial
* Plane
* Sphere 
* Distance Grab
