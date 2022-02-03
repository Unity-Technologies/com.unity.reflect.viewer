using System;
using System.Collections;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI.Utils
{
    public class PrefabListValue : MonoBehaviour, IPropertyValue<ICollection>
    {
        [SerializeField]
        GameObject m_Prefab;

        List<(GameObject gameObject, IPropertyValue propertyValue)> m_Views = new List<(GameObject, IPropertyValue)>();

        ICollection m_Values;
        public ICollection value => m_Values;

        public Type type => typeof(ICollection);
        public object objectValue
        {
            get => m_Values;

            set
            {
                m_Values = (ICollection)value;

                var valueEnumerator = m_Values.GetEnumerator();
                int index = 0;
                bool hasValueItem = valueEnumerator.MoveNext();
                while (hasValueItem || index < m_Views.Count)
                {
                    if (index >= m_Views.Count)
                    {
                        InstantiateNewItem(valueEnumerator.Current);
                    }
                    else if (hasValueItem)
                    {
                        m_Views[index].propertyValue.objectValue = valueEnumerator.Current;
                        m_Views[index].gameObject.SetActive(true);
                    }
                    else
                    {
                        m_Views[index].gameObject.SetActive(false);
                    }
                    index++;
                    hasValueItem = valueEnumerator.MoveNext();
                }
            }
        }

        void InstantiateNewItem(object value)
        {
            var view = Instantiate(m_Prefab, transform);
            var propertyValue = GetPropertyValue(view, value.GetType());
            if(view != null && propertyValue != null)
            {
                propertyValue.objectValue = value;
                m_Views.Add((view, propertyValue));
                view.gameObject.SetActive(true);
            }
        }

        IPropertyValue GetPropertyValue(GameObject gameObject, Type type)
        {
            var behaviours = gameObject.GetComponents<MonoBehaviour>();
            foreach (var monoBehaviour in behaviours)
            {
                if (monoBehaviour is IPropertyValue propertyValue && propertyValue.type == type)
                {
                    return propertyValue;
                }
            }
            return null;
        }

        public void AddListener(Action eventFunc) { }

        public void RemoveListener(Action eventFunc) { }

    }
}
