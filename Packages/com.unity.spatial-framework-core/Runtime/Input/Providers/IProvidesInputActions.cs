using Unity.XRTools.ModuleLoader;
using UnityEngine.InputSystem;

namespace Unity.SpatialFramework.Input
{
    /// <summary>
    /// Callback for action listener events. See <see cref="UsesInputActionsMethods.AddActionListeners"/> for events.
    /// </summary>
    public delegate void ActionEventCallback();

    /// <summary>
    /// Provide access to Input System InputAction functionality.
    /// </summary>
    public interface IProvidesInputActions : IFunctionalityProvider
    {
        /// <summary>
        /// Creates an <see cref="InputActionAsset"/> that is enabled and can be used to read <see cref="InputAction"/> values from. This is a clone of the input action user's <see cref="IUsesInputActions.inputActionsAsset"/>.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        /// <param name="device">The device to associate these InputActions to. This will mean that only bindings valid on the passed in device will report input. If null, or unset, will allow this functionality user to listen to all input devices.</param>
        /// <returns>The cloned, enabled InputActionAsset. This is the same asset returned to <see cref="IUsesInputActions.OnActionsCreated"/></returns>
        InputActionAsset CreateActions(IUsesInputActions user, InputDevice device = null);

        /// <summary>
        /// Removes Actions of this input user from the Input System.  This user will no longer get <see cref="IUsesInputActions.ProcessInput"/> and no action listeners will trigger.
        /// If the user calls <see cref="CreateActions"/> again, they will need to re-register action listeners, and the <see cref="InputActionAsset"/> returned from <see cref="CreateActions"/> is no longer useful.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        void RemoveActions(IUsesInputActions user);

        /// <summary>
        /// Consuming a control prevents it from calling to any other registered action callbacks until it reverts back it's default state.
        /// </summary>
        /// <param name="consumer">The functionality user that created the InputActionAsset.</param>
        /// <param name="control">The control to consume. This will affect all other InputActions that have bindings pointing to this InputControl, regardless of the binding path used.</param>
        void ConsumeControl(InputControl control, IUsesInputActions consumer);

        /// <summary>
        /// Adds a set of callbacks for a specified input action. These callbacks also exist on <see cref="InputAction"/>, however using this API allows other input users to block calls by using <see cref="UsesInputActionsMethods.ConsumeControl"/>.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        /// <param name="action">The input action to listen for changes on.</param>
        /// <param name="onStarted">A call when the input action leaves it's default state. See <see cref="InputAction.started"/></param>
        /// <param name="onPerformed">A call when the input action passes a specified threshold and is 'triggered'. See <see cref="InputAction.performed"/></param>
        /// <param name="onCanceled">A call when the input action returns to it's default state. See <see cref="InputAction.canceled"/></param>
        void AddActionListeners(IUsesInputActions user, InputAction action, ActionEventCallback onStarted = null, ActionEventCallback onPerformed = null, ActionEventCallback onCanceled = null);
    }
}
