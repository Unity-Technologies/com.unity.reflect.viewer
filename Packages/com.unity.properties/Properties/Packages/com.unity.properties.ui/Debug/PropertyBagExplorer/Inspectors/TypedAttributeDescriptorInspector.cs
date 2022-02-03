using System;
using JetBrains.Annotations;
using Unity.Properties.UI;
using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    [UsedImplicitly]
    class TypedAttributeDescriptorInspector : PropertyInspector<AttributeDescriptor<Type, string>>
    {
        public override VisualElement Build()
        {
            return new AttributeDescriptorElement<Type, string, TypeNameLabel, Label>(Target);
        }
    }
}
