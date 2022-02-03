#if !NET_DOTS
using System;
using System.Collections.Generic;

namespace Unity.Properties.Internal
{
    /// <summary>
    /// Helper visitor to visit a single property using a specified <see cref="PropertyPath"/>.
    /// </summary>
    abstract class PathVisitor : IPropertyBagVisitor, IPropertyVisitor
    {
        readonly struct PropertyScope : IDisposable
        {
            readonly PathVisitor m_Visitor;
            readonly IProperty m_Property;

            public PropertyScope(PathVisitor visitor, IProperty property)
            {
                m_Visitor = visitor;
                m_Property = m_Visitor.Property;
                m_Visitor.Property = property;
            }

            public void Dispose() => m_Visitor.Property = m_Property;
        }

        int m_PathIndex;
        public PropertyPath Path;

        public virtual void Reset()
        {
            m_PathIndex = 0;
            Path = null;
            ErrorCode = VisitErrorCode.Ok;
            ReadonlyVisit = false;
        }

        /// <summary>
        /// Returns the property for the currently visited container.
        /// </summary>
        IProperty Property { get; set; }
        public bool ReadonlyVisit { get; set; }

        public VisitErrorCode ErrorCode { get; protected set; }

        void IPropertyBagVisitor.Visit<TContainer>(IPropertyBag<TContainer> properties, ref TContainer container)
        {
            var part = Path[m_PathIndex++];

            IProperty<TContainer> property;

            switch (part.Type)
            {
                case PropertyPath.PartType.Name:
                {
                    if (properties is IPropertiesNamed<TContainer> keyable && keyable.TryGetProperty(ref container, part.Name, out property))
                    {
                        property.Accept(this, ref container);
                    }
                    else
                    {
                        ErrorCode = VisitErrorCode.InvalidPath;
                    }
                }
                    break;

                case PropertyPath.PartType.Index:
                {
                    if (properties is IPropertiesIndexed<TContainer> indexable && indexable.TryGetProperty(ref container, part.Index, out property))
                    {
                        using ((property as IAttributes).CreateAttributesScope(Property as IAttributes))
                        {
                            property.Accept(this, ref container);
                        }
                    }
                    else
                    {
                        ErrorCode = VisitErrorCode.InvalidPath;
                    }
                }
                    break;

                case PropertyPath.PartType.Key:
                {
                    if (properties is IPropertiesKeyed<TContainer, object> keyable && keyable.TryGetProperty(ref container, part.Key, out property))
                    {
                        using ((property as IAttributes).CreateAttributesScope(Property as IAttributes))
                        {
                            property.Accept(this, ref container);
                        }
                    }
                    else
                    {
                        ErrorCode = VisitErrorCode.InvalidPath;
                    }
                }
                    break;

                default:
                    ErrorCode = VisitErrorCode.InvalidPath;
                    break;
            }
        }

        void IPropertyVisitor.Visit<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container)
        {
            var value = property.GetValue(ref container);

            if (m_PathIndex >= Path.PartsCount)
            {
                VisitPath(property, ref container, ref value);
            }
            else if (PropertyBagStore.TryGetPropertyBagForValue(ref value, out _))
            {
                if (RuntimeTypeInfoCache<TValue>.CanBeNull && EqualityComparer<TValue>.Default.Equals(value, default))
                {
                    ErrorCode = VisitErrorCode.InvalidPath;
                    return;
                }
                using (new PropertyScope(this, property))
                {
                        
                    PropertyContainer.Accept(this, ref value);
                }

                if (!property.IsReadOnly && !ReadonlyVisit)
                    property.SetValue(ref container, value);
            }
            else
            {
                ErrorCode = VisitErrorCode.InvalidPath;
            }
        }

        protected virtual void VisitPath<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
        {
        }

        internal static class AOT
        {
            internal static void RegisterType<TContainer, TValue>(Property<TContainer, TValue> property, TContainer container = default, TValue value = default)
            {
                ((PathVisitor) default).VisitPath(property, ref container, ref value);
            }
        }
    }
}
#endif