using Unity.XRTools.ModuleLoader;
using UnityEngine.InputSystem;

namespace Unity.SpatialFramework.Input
{
    /// <summary>
    /// Gives a decorated class access to <see cref="InputActionAsset"/> and the action system within the Input System package.
    /// </summary>
    public interface IUsesInputActions : IFunctionalitySubscriber<IProvidesInputActions>
    {
        /// <summary>
        /// The <see cref="InputActionAsset"/> that is duplicated and bound when calling <see cref="UsesInputActionsMethods.CreateActions"/>.
        /// </summary>
        InputActionAsset inputActionsAsset { get; }

        /// <summary>
        /// Called every frame when a functionality user has active actions. Actions are active between calls to <see cref="UsesInputActionsMethods.CreateActions"/> and <see cref="UsesInputActionsMethods.RemoveActions"/>.
        /// </summary>
        /// <param name="input">The active input asset for this functionality user.</param>
        void ProcessInput(InputActionAsset input);

        /// <summary>
        /// Called as a callback to <see cref="UsesInputActionsMethods.CreateActions"/>. The returned <see cref="InputActionAsset"/> is a clone of the asset set in <see cref="IUsesInputActions.inputActionsAsset"/>.
        /// This duplicated version has all actions enabled and bound (if currently bindable).
        /// </summary>
        /// <param name="input">The active input asset for this functionality user.</param>
        void OnActionsCreated(InputActionAsset input);
    }

    /// <summary>
    /// Extension methods for implementors of IUsesInputActions
    /// </summary>
    public static class UsesInputActionsMethods
    {
        /// <summary>
        /// Consuming a control prevents it from calling to any other registered action callbacks until it reverts back it's default state.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        /// <param name="control">The control to consume. This will affect all other InputActions that have bindings pointing to this InputControl, regardless of the binding path used.</param>
        public static void ConsumeControl(this IUsesInputActions user, InputControl control)
        {
            user.provider.ConsumeControl(control, user);
        }

        /// <summary>
        /// Adds a set of callbacks for a specified input action. These callbacks also exist on <see cref="InputAction"/>, however using this API allows other input users to block calls by using <see cref="ConsumeControl"/>.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        /// <param name="action">The input action to listen for changes on.</param>
        /// <param name="onStarted">A call when the input action leaves it's default state. See <see cref="InputAction.started"/></param>
        /// <param name="onPerformed">A call when the input action passes a specified threshold and is 'triggered'. See <see cref="InputAction.performed"/></param>
        /// <param name="onCanceled">A call when the input action returns to it's default state. See <see cref="InputAction.canceled"/></param>
        public static void AddActionListeners(this IUsesInputActions user, InputAction action, ActionEventCallback onStarted = null, ActionEventCallback onPerformed = null, ActionEventCallback onCanceled = null)
        {
            user.provider.AddActionListeners(user, action, onStarted, onPerformed, onCanceled);
        }

        /// <summary>
        /// Creates an <see cref="InputActionAsset"/> that is enabled and can be used to read <see cref="InputAction"/> values from. This is a clone of the input action user's <see cref="IUsesInputActions.inputActionsAsset"/>.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        /// <param name="device">The device to associate these InputActions to. This will mean that only bindings valid on the passed in device will report input. If null, or unset, will allow this functionality user to listen to all input devices.</param>
        /// <returns>The cloned, enabled InputActionAsset. This is the same asset returned to <see cref="IUsesInputActions.OnActionsCreated"/></returns>
        public static InputActionAsset CreateActions(this IUsesInputActions user, InputDevice device = null)
        {
            return user.provider.CreateActions(user, device);
        }

        /// <summary>
        /// Removes Actions of this input user from the Input System.  This user will no longer get <see cref="IUsesInputActions.ProcessInput"/> and no action listeners will trigger.
        /// If the user calls <see cref="CreateActions"/> again, they will need to re-register action listeners, and the <see cref="InputActionAsset"/> returned from <see cref="CreateActions"/> is no longer useful.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        public static void RemoveActions(this IUsesInputActions user)
        {
            user.provider.RemoveActions(user);
        }
    }
}
