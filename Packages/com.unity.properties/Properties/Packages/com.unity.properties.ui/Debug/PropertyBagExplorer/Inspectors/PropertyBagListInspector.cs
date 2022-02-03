using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Properties.UI;
using Unity.Properties.UI.Internal;
using UnityEngine.UIElements;

namespace Unity.Properties.Debug
{
    [UsedImplicitly]
    class PropertyBagListInspector : Inspector<List<PropertyBagDebugInfo>>
    {
        const string k_Search = "unity-properties__property-bag-list__search-element";
        const string k_ListView = "unity-properties__property-bag-list__list-view";
        
        SearchElement m_SearchElement;
        ListView m_ListView;

        public override VisualElement Build()
        {
            var context = GetContext<Explorer.Context>();
            context.NewPropertyBagsDetected += () =>
            {
                m_ListView.Refresh();
                m_SearchElement.Search();
                m_ListView.selectedIndex = string.IsNullOrEmpty(m_SearchElement.value)
                    ? PropertyBagDebugInfoStore.IndexOf(context.SelectedType)
                    : Target.FindIndex(pbd => pbd.Type == context.SelectedType);
            };
            var root = Resources.Templates.Explorer.PropertyBagList.CloneWithoutTemplateContainer();

            m_SearchElement = root.Q<SearchElement>(className: k_Search);
            m_SearchElement.RegisterCallback<ChangeEvent<string>, Explorer.Context>(
                (evt, ctx) => ctx.StringSearch = evt.newValue, context);
            m_SearchElement.RegisterSearchQueryHandler<PropertyBagDebugInfo>(search =>
            {
                Target.Clear();
                Target.AddRange(search.Apply(PropertyBagDebugInfoStore.AllDebugInfos));
                m_ListView.Refresh();
                m_ListView.selectedIndex = string.IsNullOrEmpty(m_SearchElement.value)
                    ? PropertyBagDebugInfoStore.IndexOf(context.SelectedType)
                    : Target.FindIndex(pbd => pbd.Type == context.SelectedType);
            });

            m_ListView = root.Q<ListView>(className: k_ListView);
            m_ListView.selectionType = SelectionType.Single;
            m_ListView.itemsSource = Target;
            m_ListView.makeItem = () =>
            {
                var element = new TypeNameLabel();
                element.style.paddingLeft = 15;
                return element;
            };
            m_ListView.bindItem = (element, i) =>
            {
                if (element is TypeNameLabel typeName)
                    typeName.value = Target[i].Type;
            };

            m_SearchElement.value = context.StringSearch;

            context.Update();
            m_ListView.selectedIndex = string.IsNullOrEmpty(m_SearchElement.value)
                ? PropertyBagDebugInfoStore.IndexOf(context.SelectedType)
                : Target.FindIndex(pbd => pbd.Type == context.SelectedType);

            context.OnPropertyBagSelected += detail =>
            {
                context.Update();
                m_ListView.selectedIndex = string.IsNullOrEmpty(m_SearchElement.value)
                    ? PropertyBagDebugInfoStore.IndexOf(context.SelectedType)
                    : Target.FindIndex(pbd => pbd.Type == context.SelectedType);
                m_ListView.ScrollToItem(m_ListView.selectedIndex);
            };
            
            m_ListView.onSelectionChange += objects =>
            {
                foreach (var obj in objects)
                {
                    if (obj is PropertyBagDebugInfo detail)
                    {
                        context.SelectPropertyBag(detail);
                        return;
                    }
                }
            };

            return root;
        }
    }
}
