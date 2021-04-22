namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Implementors can have objects dropped onto them. See DragAndDropModule for more information
    /// </summary>
    public interface IDropReceiver
    {
        /// <summary>
        /// Called when an object is hovering over the receiver
        /// </summary>
        /// <param name="dropObject">The object we are dropping</param>
        /// <returns>Whether the drop can be accepted</returns>
        bool CanDrop(object dropObject);

        /// <summary>
        /// Called when a pointer with a valid drop object starts hovering
        /// </summary>
        void OnDropHoverStarted();

        /// <summary>
        /// Called when a pointer with a valid drop object stops hovering
        /// </summary>
        void OnDropHoverEnded();

        /// <summary>
        /// Called when an object is dropped on the receiver
        /// </summary>
        /// <param name="dropObject">The object we are dropping</param>
        /// <returns>Whether the drop was accepted</returns>
        void ReceiveDrop(object dropObject);
    }
}
