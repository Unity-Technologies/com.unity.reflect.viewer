namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Interface for components that control pooled UI elements
    /// </summary>
    interface IPooledUI
    {
        /// <summary>
        /// Whether the UI instance is in the active or pooled waiting to be used.
        /// </summary>
        bool active { get; set; }

        /// <summary>
        /// Called when the pool wants to destroy this instance.
        /// </summary>
        void Destroy();
    }
}
