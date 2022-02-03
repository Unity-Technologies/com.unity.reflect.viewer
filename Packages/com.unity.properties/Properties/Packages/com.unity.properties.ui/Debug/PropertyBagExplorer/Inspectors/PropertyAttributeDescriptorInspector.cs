using System.Linq;
using JetBrains.Annotations;
using Unity.Properties.Editor;
using Unity.Properties.Internal;
using Unity.Properties.UI;
using Unity.Properties.UI.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    [UsedImplicitly]
    class PropertyAttributeDescriptorInspector : PropertyInspector<PropertyTypeDescriptor>
    {
        const string k_Attributes = "unity-properties__property__attributes";
        
        public override VisualElement Build()
        {
            var root = new VisualElement();
            var attributes =
                $"[{string.Join(", ", Target.Descriptor.GetAttributes().Select(a => TypeUtility.GetTypeDisplayName(a.GetType())))}]";
            if (attributes.Length > 2)
            {
                var attributesLabel = new Label(attributes);
                Resources.Templates.Explorer.Property.AddStyles(attributesLabel);
                attributesLabel.AddToClassList(UssClasses.Unity.BaseFieldLabel);
                attributesLabel.AddToClassList(k_Attributes);
                root.contentContainer.Add(attributesLabel);
            }

            var descriptor = new AttributeDescriptorElement<string, string, Label, Label>(new AttributeDescriptor<string, string>(Target.Descriptor.Name,
                TypeUtility.GetTypeDisplayName(Target.Value)));
            var type = Target.Descriptor.DeclaredValueType();
            if (RuntimeTypeInfoCache.IsContainerType(type)
                && !type.IsAbstract
                && !type.IsInterface)
            {
                descriptor.ShowValueAsLink();
            }

            descriptor.RegisterCallback<TypeSelectedEvent<AttributeDescriptor<string, string>>>(evt =>
            {
                GetContext<Explorer.Context>().SelectType(Target.Value);
            });

            root.contentContainer.Add(descriptor);
            return root;
        }
    }
}
