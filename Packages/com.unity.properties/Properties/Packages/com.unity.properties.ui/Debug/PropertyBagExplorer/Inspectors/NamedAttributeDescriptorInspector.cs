using JetBrains.Annotations;
using Unity.Properties.UI;
using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    [UsedImplicitly]
    class NamedAttributeDescriptorInspector : PropertyInspector<AttributeDescriptor<string, string>>
    {
        public override VisualElement Build()
        {
            return new AttributeDescriptorElement<string, string, Label, Label>(Target);
        }
    }
}
