using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Properties.UI;
using Unity.Properties.UI.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    [UsedImplicitly]
    class InlineListInspector<T> : PropertyInspector<List<T>, InlineListAttribute>, IExperimentalInspector
    {
        const string k_Section = "unity-properties__property-bag__section";
        const string k_Content = "unity-properties__property-bag__content";
        const string k_Empty = "unity-properties__property-bag__none";
        
        public override VisualElement Build()
        {
            var foldout = new Foldout {text = DisplayName};
            Resources.Templates.Explorer.PropertyBag.AddStyles(foldout);
            foldout.AddToClassList(k_Section);
            foldout.contentContainer.AddToClassList(k_Content);

            for(var i = 0; i < Target.Count; ++i)
                DoDefaultGuiAtIndex(foldout.contentContainer, i);

            if (Target.Count == 0 && !string.IsNullOrEmpty(DrawerAttribute.MessageWhenEmpty))
            {
                var label = new Label(DrawerAttribute.MessageWhenEmpty);
                label.AddToClassList(k_Empty);
                foldout.contentContainer.Add(label);
            }
            return foldout;
        }
    }
}
