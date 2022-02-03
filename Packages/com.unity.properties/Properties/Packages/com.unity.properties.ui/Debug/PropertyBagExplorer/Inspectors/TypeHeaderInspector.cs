using System;
using JetBrains.Annotations;
using Unity.Properties.UI;
using Unity.Properties.UI.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    [UsedImplicitly]
    class TypeHeaderInspector : PropertyInspector<Type, TypeHeaderAttribute>
    {
        const string k_Header = "unity-properties__property-bag__header";
        const string k_TypeName = "unity-properties__property-bag__type-name";
        
        public override VisualElement Build()
        {
            var root = new VisualElement();
            Resources.Templates.Explorer.PropertyBag.AddStyles(root);
            root.AddToClassList(UssClasses.Common.Expand);
            root.AddToClassList(k_Header);
             
            var typeName = new TypeNameLabel(Target);
            typeName.AddToClassList(UssClasses.Unity.BaseFieldLabel);
            typeName.AddToClassList(k_TypeName);
            root.Add(typeName);
            return root;
        }
    }
}
