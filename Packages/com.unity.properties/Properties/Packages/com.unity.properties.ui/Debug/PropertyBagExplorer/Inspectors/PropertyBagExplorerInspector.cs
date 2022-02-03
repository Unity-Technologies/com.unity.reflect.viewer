using JetBrains.Annotations;
using Unity.Properties.UI;
using Unity.Properties.UI.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    [UsedImplicitly]
    class PropertyBagExplorerInspector : Inspector<Explorer>
    {
        const string k_Splitter = "unity-properties__property-bag-explorer__splitter";
        const string k_Detail = "unity-properties__property-bag-explorer__detail";
        
        InspectorElement m_CurrentPropertyBag;

        public override VisualElement Build()
        {
            var root = new VisualElement();
            root.StretchToParentSize();
            Resources.Templates.Explorer.PropertyBagExplorer.Clone(root);
            var pooled = ListPool<BindingContextElement>.Get();
            try
            {
                root.Query<BindingContextElement>().ToList(pooled);
                foreach (var binding in pooled)
                {
                    binding.AddContext(Target.InspectionContext);
                }
            }
            finally
            {
                ListPool<BindingContextElement>.Release(pooled);
            }

            var split = root.Q<TwoPaneSplitView>(className:k_Splitter);
            split.fixedPaneInitialDimension = Target.InspectionContext.SplitPosition; 
            split.schedule.Execute(() =>
            {
                if (null != split.fixedPane)
                    Target.InspectionContext.SplitPosition = split.fixedPane.resolvedStyle.width;
            }).Every(0);

            Target.InspectionContext.OnPropertyBagSelected += detail =>
            {
                m_CurrentPropertyBag.ClearTarget();
                m_CurrentPropertyBag.SetTarget(detail);
            };
 
            m_CurrentPropertyBag = root.Q<InspectorElement>(className: k_Detail);
            return root;
        }

        public override void Update()
        {
            Target.InspectionContext.Update();
        }
    }
}
