namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// How the manipulator affects multiple objects.
    /// </summary>
    public enum PivotGrouping
    {
        /// <summary>
        /// Each object is rotated and scaled around a shared point.
        /// </summary>
        Group,
        /// <summary>
        /// Each object is rotated and scaled around their individual pivot or center point.
        /// </summary>
        Individual
    }
}
