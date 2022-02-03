# Transform Manipulators

Transform Manipulators are a group of handles for modifying the position, rotation, and scale of objects.

## Showing the Manipulator

In most cases, there is only 1 group of manipulators acting on 1 set of selected objects.

To show the manipulator use the interface **IUsesTransformManipulators**
```
this.SetManipulatorsVisible(this, true); // Adds this to the list of objects that need the manipulator visible  
```
When something sets the manipulator visible, it is added to a list of current manipulator requests. If there is at least one user in the list, the manipulator will be visible.

The manipulators also needs to be assigned a selection of objects and one object in that list to be the active selection. The active selection is used for choosing the rotation of the manipulators.

```
Transform[] selectionList = // List of transforms 
Transform activeTransform = selectionList[0];
this.SetManipulatorSelection(selectionList, activeTransform);
```

To hide the manipulator, all users must call the method again with the argument false to be removed from the list.
```
this.SetManipulatorsVisible(this, false); // removes this to the list of objects that need the manipulator visible  
```

## Switching Manipulators

The **ManipulatorModule** asset located in `Assets/SpatialFramework/Settings/Resources/ManipulatorModule` references the list of manipulator prefabs. 

It also contains a list of named groups that can be switched between. Each group lists the indices of the prefabs that are part of the group.

The default groups are:

* **Bounding Box** : Bounding box that can be grabbed and moved.
  * Reaching or retracting the controller will push or pull the object further or closer.
  * If available, the joystick X and Y will spin (rotate around world Y axis) and uniform scale the objects.   
 
* **Standard Handles** : Translate 1D, Rotate, and Scale 1D handles

To set the current group by name:

```
this.SetManipulatorGroup("Bounding Box"); // Sets the manipulator group to Bounding Box
```

To cycle between groups:

```
this.NextManipulatorGroup(); // Switches to the next manipulator group
```


## Standard Transform Manipulators

These manipulators are the common handles seen in 3D applications such as the Unity Editor, but adapted and styled for spatial (6 dof) controllers.

* Translate 1D

* Translate 2D

* Scale 1D

* Rotate

## Other Manipulators

* Physics Based Bounding Box

## Manipulators in the Scene

In addition to the main manipulator accessed via `IUsesTransformManipulators`, additional manipulator can be created to control a set of transforms by adding the `ManipulatorSelectionController` component.
