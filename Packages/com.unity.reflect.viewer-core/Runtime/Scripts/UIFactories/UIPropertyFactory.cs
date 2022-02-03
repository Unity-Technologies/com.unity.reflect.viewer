using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using Unity.TouchFramework;
using UnityEngine;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class UIPropertyFactory : MonoBehaviour, IContextPropertyFactory
    {
#pragma warning disable CS0649
        [SerializeField]
        List<GameObject> m_RuntimePrefabs;
#pragma warning restore CS0649

        Dictionary<Type, GameObject> m_PrefabsDictionary;
        bool m_bInitialized;

        void Start()
        {
            if (!m_bInitialized)
                InitializeFactory();
        }

        void InitializeFactory()
        {
            m_PrefabsDictionary = new Dictionary<Type, GameObject>();
            foreach (var prefab in m_RuntimePrefabs)
            {
                var iPropertyValue = prefab.GetComponentInChildren<IPropertyValue>();
                m_PrefabsDictionary[iPropertyValue.type] = prefab;
            }

            m_bInitialized = true;
        }

        public GameObject CreateInstance(Type contextType, IProperty property)
        {
            if (!m_bInitialized)
                InitializeFactory();

            if (!m_PrefabsDictionary.ContainsKey(property.DeclaredValueType()))
                throw new InvalidCastException($"The current PrefabFactory does not contain an entry for type {property.DeclaredValueType().FullName} for context `{contextType.Name}` and property `{property.Name}`");
            var newPrefab = GameObject.Instantiate(m_PrefabsDictionary[property.DeclaredValueType()]);
            var uiSelector = newPrefab.AddComponent<UISelectorComponent>();
            uiSelector.ContextTypeName = contextType.Name;
            uiSelector.PropertyName = property.Name;
            uiSelector.BindContext();

            return newPrefab;
        }
    }

    public class ContextPropertyContainer : MonoBehaviour
    {

    }

    public interface IContextPropertyFactory
    {
        GameObject CreateInstance(Type propertyDataContextType, IProperty propertyDataProperty);
    }
}
