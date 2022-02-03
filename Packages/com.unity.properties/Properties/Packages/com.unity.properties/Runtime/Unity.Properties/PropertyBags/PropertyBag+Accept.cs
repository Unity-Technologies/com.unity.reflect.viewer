using System;

namespace Unity.Properties
{
    public static partial class PropertyBag
    {
        internal static void AcceptWithSpecializedVisitor<TContainer>(IPropertyBag<TContainer> properties, IPropertyBagVisitor visitor, ref TContainer container)
        {
            switch (properties)
            {
                case IDictionaryPropertyBagAccept<TContainer> accept when visitor is IDictionaryPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                case IListPropertyBagAccept<TContainer> accept when visitor is IListPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                case ISetPropertyBagAccept<TContainer> accept when visitor is ISetPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                case ICollectionPropertyBagAccept<TContainer> accept when visitor is ICollectionPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                case IPropertyBagAccept<TContainer> accept when visitor is IPropertyBagVisitor typedVisitor:
                    accept.Accept(typedVisitor, ref container);
                    break;
                default:
                    throw new ArgumentException($"{visitor.GetType()} does not implement any IPropertyBagAccept<T> interfaces.");
            }
        }
    }
}