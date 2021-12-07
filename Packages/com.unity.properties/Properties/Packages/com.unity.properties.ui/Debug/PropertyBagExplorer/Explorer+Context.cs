using System;
using Unity.Properties.Internal;
using Unity.Properties.UI;
using Unity.Serialization;
using UnityEngine;

namespace Unity.Properties.Debug
{
    partial class Explorer
    {
        public class Context : InspectionContext
        {
            int m_LastPropertyBagCount;

            public Context()
            {
                m_LastPropertyBagCount = PropertyBagDebugInfoStore.AllDebugInfos.Count;
            }

            public event Action NewPropertyBagsDetected = delegate { };
            public event Action<PropertyBagDebugInfo> OnPropertyBagSelected = delegate { };

            public string StringSearch;

            [CreateProperty, HideInInspector]
            string SelectedTypeName
            {
                get => SelectedType?.AssemblyQualifiedName;
                set
                {
                    if (string.IsNullOrEmpty(value))
                        return;

                    var type = Type.GetType(value);
                    if (null != type)
                    {
                        // Force generation of the property bag
                        PropertyBagStore.GetPropertyBag(type);
                        SelectedType = type;
                        return;
                    }

                    if (!FormerNameAttribute.TryGetCurrentTypeName(value, out var newTypeName))
                        return;

                    type = Type.GetType(newTypeName);
                    if (null == type)
                        return;

                    // Force generation of the property bag
                    PropertyBagStore.GetPropertyBag(type);
                    SelectedType = type;
                }
            }

            [CreateProperty, HideInInspector] public float SplitPosition = 250;

            Type m_Type;

            public Type SelectedType
            {
                get
                {
                    if (null == m_Type)
                        m_Type = PropertyBagDebugInfoStore.AllDebugInfos[0].Type;
                    return m_Type;
                }
                private set => m_Type = value;
            }

            public void SelectType(Type type)
            {
                if (!RuntimeTypeInfoCache.IsContainerType(type))
                    return;

                if (type.IsAbstract || type.IsInterface)
                    return;

                var existed = PropertyBagStore.Exists(type);
                var bag = PropertyBagStore.GetPropertyBag(type);
                if (null == bag)
                    return;

                if (!existed)
                    Update();

                if (!PropertyBagDebugInfoStore.TryGetPropertyBagDetail(type, out var details)) return;
                SelectPropertyBag(details);
            }

            public void SelectPropertyBag(PropertyBagDebugInfo propertyBag)
            {
                if (SelectedType == propertyBag.Type)
                    return;
                SelectedType = propertyBag.Type;
                OnPropertyBagSelected.Invoke(propertyBag);
            }

            public void Update()
            {
                if (m_LastPropertyBagCount == PropertyBagStore.AllTypes.Count)
                    return;
                m_LastPropertyBagCount = PropertyBagDebugInfoStore.AllDebugInfos.Count;

                NewPropertyBagsDetected.Invoke();
            }
        }
    }
}
