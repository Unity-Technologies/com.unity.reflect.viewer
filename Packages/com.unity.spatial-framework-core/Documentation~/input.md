# Spatial Input

Input in Spatial is built off of Unity's [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.1/manual/) package and builds off those concepts. Spatial adds support for per-control input priorities, a single action map with per-device binding callbacks, and writing XR controller bindings based on a user's dominant hand.

## Using Input Actions

In order to get access to input data, systems must implement *IUsesInputActions*.

Implementors of *IUsesInputActions* must specify their InputActionAsset via the `inputActionAsset` property. This will be used when calling `InputActionAsset CreateActions(InputDevice device = null)`. When providing an InputDevice, bindings will only be supported for that specific device. When unset, or set to null, InputActions on the implementor can be bound to any connected InputDevice. The created InputActionAsset is a duplicate of the one provided, and will be returned by the CreateActions call. The implementor will also receive a callback in the form of `void OnActionsCreated(InputActionAsset inputs)`. That new asset will be enabled and ready to poll inputs from.

Once the implementor has active inputs, they can use actions in a few ways.
```
InputAction m_MyInputAction;

void OnActionsCreated(InputActionAsset input)
{
    // This asset has a declared input action named "MyInputAction"
    m_MyInputAction = input.FindAction("MyInputAction");

    // You can register to be notified only on input changes.
    this.AddActionListeners(m_MyInputAction, onStarted: OnActionStarted, onPerformed: OnActionPerformed, onCanceled: OnActionCancelled)
}

// You can also use the process input callback to get an update loop after all inputs have been processed each frame.
void ProcessInput(InputActionAsset action)
{
    if(m_MyInputAction != null)
        var actionValue = m_MyInputAction.ReadValue<float>();
}

// A callback triggered whenever the InputAction is performed.
void OnActionPerformed()
{
    var actionValue = m_MyInputAction.ReadValue<float>();

}
```

For action listeners, started refers to the moment the action leaves it's default state (e.g. when a thumbstick leaves it's deadzone), performed refers to when it is considered actuated (e.g. when a trigger passes a 'click' threshold), and cancelled refers to when the input reverts back to it's default state (e.g. when a button is released). These are the same states as within the core Input System.

When calling `AddActionListeners` you only need to register for the events you are interested.  Null parameters will simply be ignored.

Inputs can also be consumed. Consuming an input means that no other user will be able to get inputs from the active control until it reverts back to it's default state (i.e. cancelled is called). It is useful to use the InputAction's `.activeControl` property to get the currently active control triggering the action.

```
void OnActionPerformed()
{
    // This will prevent any other InputActions from getting input data from the control that triggered m_MyInputAction.
    this.ConsumeControl(m_MyInputAction.activeControl);

}
```

## Device Handedness

Device handedness let's you bind inputs against a dominant hand, letting the underlying system decide if the user is left or right handed. The DeviceInputModule will add a new _Dominant Hand_ and _Off Hand_ to usage to all Input Device bindings in the Input System. These can also be referenced by Input System path tag `{DominantHand}` and `{OffHand}` respectively.

![Dominant Hand Usages](images/Input/HandedUsages.png)

When the system is set to *Right Handed*, the dominant hand usage is on devices also tagged with *RightHand*, and the off hand usage is tagged on devices tagged *LeftHand*.  The inverse is true when set to *Left Handed* mode. This let's the developer, or users configure they're preferred hand independent of the action map setup.  These usages are set in the Input System, and so existing APIs, such as `InputSystem.GetDevice(string usage)` can also be used to find dominant or off handed devices.

Use *IUsesDeviceHandedness* to get, set, and be notified of handedness changes. Implementors get access to basic accessors such as: `XRControllerHandedness GetHandedness()` and `void SetHandedness(XRControllerHandedness handedness)`, which allow for basic control of the current handedness.  Implementors can also use `void SubscribeToHandednessChanges(XRControllerHandedness handedness` to get notified when the handedness changes, which will also signify the underlying input bindings have likely changed.  All Input System InputActions that are already enabled will automatically update on the same frame to use the correct hand without special consideration.

## Debugging Input

If there is an issue getting input, it is recommended to open the Input Debugger which is found under **Window > Analysis > Input Debugger**. 

The debugger shows the current devices and actions, and which controls are bound to each action.

It is also suggested to enable **Options > Lock Input to Game View** in the Input Debugger window. This ensures input will occur even when the Game View does not have focus.
