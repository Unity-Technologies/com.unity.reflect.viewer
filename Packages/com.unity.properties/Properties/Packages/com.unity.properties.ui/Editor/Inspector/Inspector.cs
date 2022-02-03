namespace Unity.Properties.UI
{
    interface IRootInspector
    {
    }
    
    /// <summary>
    /// Base class for defining a custom inspector for values of type <see cref="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value to inspect.</typeparam>
    public abstract class Inspector<T> : InspectorBase<T>, IRootInspector
    {
        
    }
}