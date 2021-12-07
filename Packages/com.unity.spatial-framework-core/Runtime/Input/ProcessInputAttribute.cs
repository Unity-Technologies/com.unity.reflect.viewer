using System;

namespace Unity.SpatialFramework.Input
{
    /// <summary>
    /// This attribute specifies the order event callbacks and ProcessInput are called on a class.
    /// This can be used to make sure one class can consume input controls before another class can respond to it.
    /// A lower number will give this class access to input events and processing before other input users.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProcessInputAttribute : Attribute
    {
        public int order;

        public ProcessInputAttribute(int order)
        {
            this.order = order;
        }
    }
}
