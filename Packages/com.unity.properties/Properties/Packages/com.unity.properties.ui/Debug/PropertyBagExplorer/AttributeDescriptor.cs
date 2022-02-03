using System;

namespace Unity.Properties.Debug
{
    interface IAttributeDescriptor
    {
    }
    
    class AttributeDescriptor<TDescriptor, TValue> : IAttributeDescriptor
    {
        public AttributeDescriptor(TDescriptor descriptor, TValue value)
        {
            Descriptor = descriptor;
            Value = value;
        }
        
        public TDescriptor Descriptor;
        public TValue Value;
    }

    static class AttributeDescriptor
    {
        public static AttributeDescriptor<TDescriptor, TValue> Make<TDescriptor, TValue>(TDescriptor descriptor,
            TValue value)
        {
            return new AttributeDescriptor<TDescriptor, TValue>(descriptor, value);
        }
    }

    class PropertyTypeDescriptor : AttributeDescriptor<IProperty, Type>
    {
        public PropertyTypeDescriptor(IProperty property, Type value) : base(property, value)
        {
        }
    }
}
