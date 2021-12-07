using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class GenericUIController : DynamicUIController
    {
#pragma warning disable CS0649
        [SerializeField]
        List<string> m_OnlyContextsAllowed;

        [SerializeField]
        List<string> m_OnlyPropertiesAllowed;
#pragma warning restore CS0649
        void Start()
        {
            m_HandlerList = new List<HandlerData>();

            var propertyFactory = GetComponent<IContextPropertyFactory>();
            if (propertyFactory == null)
            {
                // check if any parent has one
                propertyFactory = GetComponentInParent<IContextPropertyFactory>();
            }

            var propertyProviders = GetComponents<IContextPropertyProvider>();
            if (propertyProviders == null || propertyProviders.Length == 0)
            {
                // check if any parent has one
                propertyProviders = GetComponentsInParent<IContextPropertyProvider>();
            }

            var insertionComponent = GetComponent<IContextContainer>();
            if (insertionComponent == null)
            {
                // check if any child has it
                insertionComponent = GetComponentInChildren<IContextContainer>();
            }

            var insertionPoint = transform;
            if (insertionComponent != null)
            {
                insertionPoint = insertionComponent.GetInsertionPoint();
            }

            if (propertyFactory != null && propertyProviders != null && propertyProviders.Length > 0)
            {
                foreach (var provider in propertyProviders)
                {
                    foreach (var propertyData in provider.GetProperties())
                    {
                        if (
                            (m_OnlyContextsAllowed.Count == 0 || (m_OnlyContextsAllowed.Contains(propertyData.context.Name) || m_OnlyContextsAllowed.Contains(propertyData.context.FullName))) &&
                            (m_OnlyPropertiesAllowed.Count == 0 || m_OnlyPropertiesAllowed.Contains(propertyData.property.Name)))
                        {
                            var propertyWidget = propertyFactory.CreateInstance(propertyData.context, propertyData.property);
                            propertyWidget.transform.SetParent(insertionPoint);
                            propertyWidget.transform.localScale = Vector3.one;
                        }
                    }
                }
            }

            // now we can bind to those components
            BindSelectors(true);
        }
    }
}
