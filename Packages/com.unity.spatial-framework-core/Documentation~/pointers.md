# XR UI Pointers

Pointers for interacting with user interface objects via the Event System.

## Setting up XR Interaction UI
The XR Interaction Toolkit drives the base input to the Event System.

* In the scene, add an Event System with an XR UI Input Module. 
* Also add an Interactor Physics Raycaster if you need UI events to be sent to 3D (non-canvas) gameobjects.
![Event System components setup](images/Pointers/event-system.png)


* On all canvases, set event camera to Main Camera and add a Tracked Device Graphic Raycaster

* In the scene under the XR Rig, either create or find an existing `XR Controller (Action Based)` with a `Ray Interactor`.
    * On the Spatial Framework Scene Controller, add the controller to the list `Device Pointers` and specify which Input System usage to bind to (such as RightHand or LeftHand).
    * Instead of setting up the pointers/controllers in the scene, [scripts can create new pointers](#CreatingPointers).

### Changing Input Actions
The **XRUIPointerProvider** asset located in `Assets/SpatialFramework/Settings/Resources/XRUIPointerProvider` references the InputActionAsset used when setting up an XR Pointer.
This asset can be duplicated and modified to customize the input bindings.

## Interfacing with Pointer system

The Spatial Framework will mediate creating pointers and provide ways to interface with them.
The pointers are driven via XR Ray Interactors set to use the UI layer.

<a name="CreatingPointers"></a>
### Create and Reference Pointers
To create a pointer, use the interface IUsesXRPointers.

```
var inputDevice = InputSystem.GetDevice<UnityEngine.InputSystem.XR.XRController>("RightHand");
var xriController = this.CreateXRPointer(xrController, Camera.main.transform.parent, out var pointerId, EnableRaycastUI);
...

bool EnableRaycastUI(IRaycastSource source)
{ 
    return true; // Can disable the pointer by returning false
}
```

If you already have an Action Based XR Controller and Ray Interactor in the scene, you can provide it instead of it being created from scratch.

```
myControllerGameObject = // Some existing gameObject in the scene
var xriController = this.CreateXRPointer(xrController, myControllerGameObject.transform.parent, out var pointerId, EnableRaycastUI, myControllerGameObject);

```

Other methods available via the interface include: 
* GetPointerIDForRayOrigin
* GetDeviceForPointerID
* BlockUIInteraction
* IsHoveringOverUI


### Reacting to UI events (per object)
If you want a particular UI element to react to UI events, you should use the standard UGUI event system components such as Button, Event Trigger, etc.

Or write a script that implements the IPointerEnterHandler, IPointerClickHandler, etc.

### Reacting to UI events (all objects)
To react to all UI events, use the interface IUsesUIEvents. Then use extension methods such as SubscribeToDragEnded.

